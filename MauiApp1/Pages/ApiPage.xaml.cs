using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Text.Json;
using System.Linq.Expressions;

namespace MauiApp1.Pages
{
    // modelo simples para o JSON retornado pela API de estados do IBGE
    public class EstadoDto
    {
        public int id { get; set; }
        public string nome { get; set; }
        public string sigla { get; set; }
    }

    public partial class ApiPage : ContentPage
    {
        private readonly HttpClient httpClient = new HttpClient();

        // URL pública IBGE - Localidades (estados)
        private const string EndpointEstados = "https://servicodados.ibge.gov.br/api/v1/localidades/estados";

        public ApiPage()
        {
            InitializeComponent();
            // carrega automaticamente ao abrir
            _ = LoadEstadosAsync();
        }

        private async Task LoadEstadosAsync()
        {
            try
            {
                // UI: mostra indicador e limpa mensagens
                loadingIndicator.IsVisible = true;
                loadingIndicator.IsRunning = true;
                errorLabel.IsVisible = false;
                itemsView.ItemsSource = null;

                // Chamada GET simples
                var response = await httpClient.GetAsync(EndpointEstados);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var estados = JsonSerializer.Deserialize<List<EstadoDto>>(json, options) ?? new List<EstadoDto>();

                // opcional: ordenar por nome (a API tem query orderBy, mas aqui fazemos local)
                estados.Sort((a, b) => string.Compare(a.nome, b.nome, StringComparison.InvariantCultureIgnoreCase));

                // liga à CollectionView
                itemsView.ItemsSource = estados;
            }
            catch (Exception ex)
            {
                // mostra erro simples na UI
                errorLabel.Text = "Erro: " + ex.Message;
                errorLabel.IsVisible = true;
            }
            finally
            {
                loadingIndicator.IsRunning = false;
                loadingIndicator.IsVisible = false;
            }
        }
        private async void RefreshButton_Clicked(object sender, EventArgs e)
        {
            await LoadEstadosAsync();
        }
        private async void MinhaUFButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Busca atual localização do dispositivo
                var location = await Geolocation.Default.GetLocationAsync();

                if (location != null)
                {
                   using var client = new HttpClient();
                    var url = $"https://nominatim.openstreetmap.org/reverse?lat={location.Latitude}&lon={location.Longitude}&format=json&accept-language=pt";
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("MauiApp1");

                    var json = await client.GetStringAsync(url);
                    using var doc = JsonDocument.Parse(json);

                    var address = doc.RootElement.GetProperty("address");

                    string cidade = "";
                    string estado = "";

                    if (address.TryGetProperty("city", out var cityProp))
                        cidade = cityProp.GetString();

                    if (string.IsNullOrEmpty(cidade) && address.TryGetProperty("town", out var townProp))
                        cidade = townProp.GetString();

                    if (string.IsNullOrEmpty(cidade) && address.TryGetProperty("village", out var villageProp))
                        cidade = villageProp.GetString();


                    if (address.TryGetProperty("state", out var stateProp))
                        estado = stateProp.GetString();

                    await DisplayAlert("Minha Localização", $"Cidade: {cidade}\n" +
                                                                  $"UF: {estado}\n" +
                                                                  $"Latitude: {location.Latitude}\n" +
                                                                  $"Longitude: {location.Longitude}\n", "OK");

                    }
                    else
                    {
                    await DisplayAlert("Ops!", "Não foi possível determinar a UF a partir da localização.", "OK");
                    }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ops!", $"Não foi possível obter a localização: {ex.Message}", "OK");
            }
        }
    }
}
