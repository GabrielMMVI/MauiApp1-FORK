using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using MauiApp1.Models;
using System.Text.Json;

namespace MauiApp1.Pages
{

    public partial class ListaPage : ContentPage
    {

        private async void AbrirApiPage_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ApiPage());
        }

        // lista completa (origem dos dados)
        private List<Item> allItems = new List<Item>();

        // coleção visível (binding para CollectionView)
        private ObservableCollection<Item> filteredItems = new ObservableCollection<Item>();

        public ListaPage()
        {
            InitializeComponent();

            // carrega dados do JSON embutido
            LoadItemsFromEmbeddedJson();

            // liga o ItemsSource do CollectionView
            itemsView.ItemsSource = filteredItems;

            // atualiza contador
            UpdateCountLabel();
        }

        private void LoadItemsFromEmbeddedJson()
        {
            // JSON embutido com 20 itens (Id, Title, Subtitle, Description)
            var json = @"
[
  { ""Id"":1,  ""Title"":""Maçã"",          ""Subtitle"":""Fruta vermelha"",   ""Description"":""Uma maçã suculenta."" },
  { ""Id"":2,  ""Title"":""Banana"",        ""Subtitle"":""Fruta amarela"",    ""Description"":""Boa para energia rápida."" },
  { ""Id"":3,  ""Title"":""Laranja"",       ""Subtitle"":""Cítrica"",          ""Description"":""Rica em vitamina C."" },
  { ""Id"":4,  ""Title"":""Pera"",          ""Subtitle"":""Fruta doce"",       ""Description"":""Textura macia."" },
  { ""Id"":5,  ""Title"":""Uva"",           ""Subtitle"":""Pequena e doce"",   ""Description"":""Ótima como snack."" },
  { ""Id"":6,  ""Title"":""Abacaxi"",       ""Subtitle"":""Tropical"",         ""Description"":""Sabor marcante."" },
  { ""Id"":7,  ""Title"":""Manga"",         ""Subtitle"":""Exótica"",          ""Description"":""Perfeita para suco."" },
  { ""Id"":8,  ""Title"":""Morango"",       ""Subtitle"":""Pequena e vermelha"",""Description"":""Muito usada em sobremesas."" },
  { ""Id"":9,  ""Title"":""Melão"",         ""Subtitle"":""Refrescante"",      ""Description"":""Boa para o verão."" },
  { ""Id"":10, ""Title"":""Kiwi"",          ""Subtitle"":""Ácido e doce"",     ""Description"":""Sementes comestíveis."" },
  { ""Id"":11, ""Title"":""Mamão"",         ""Subtitle"":""Macio"",            ""Description"":""Ajuda na digestão."" },
  { ""Id"":12, ""Title"":""Coco"",          ""Subtitle"":""Tropical"",         ""Description"":""Água e polpa versáteis."" },
  { ""Id"":13, ""Title"":""Limão"",         ""Subtitle"":""Cítrico forte"",    ""Description"":""Ótimo para temperos."" },
  { ""Id"":14, ""Title"":""Ameixa"",        ""Subtitle"":""Doce e suculenta"", ""Description"":""Boa no café da manhã."" },
  { ""Id"":15, ""Title"":""Tangerina"",     ""Subtitle"":""Fácil de descascar"", ""Description"":""Pequenas gomos suculentos."" },
  { ""Id"":16, ""Title"":""Cereja"",        ""Subtitle"":""Pequena e vermelha"", ""Description"":""Sabor marcante e festa."" },
  { ""Id"":17, ""Title"":""Framboesa"",     ""Subtitle"":""Fruta silvestre"",  ""Description"":""Delicada e perfumada."" },
  { ""Id"":18, ""Title"":""Amora"",         ""Subtitle"":""Silvestre"",        ""Description"":""Ótima para geleias."" },
  { ""Id"":19, ""Title"":""Pêssego"",       ""Subtitle"":""Suculento"",        ""Description"":""Perfeito para sobremesas."" },
  { ""Id"":20, ""Title"":""Acerola"",       ""Subtitle"":""Muito vitamina C"", ""Description"":""Pequena e bem ácida."" }
]
";

            try
            {
                // desserializa JSON para List<Item>
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var items = JsonSerializer.Deserialize<List<Item>>(json, options) ?? new List<Item>();

                allItems = items;

                // popula a coleção observável com todos os itens inicialmente
                filteredItems.Clear();
                foreach (var it in allItems)
                    filteredItems.Add(it);
            }
            catch (Exception ex)
            {
                // tratamento simples de erro (apenas debug)
                Console.WriteLine("Erro ao desserializar JSON: " + ex.Message);
            }
        }

        // evento chamado quando o texto do SearchBar muda
        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = e.NewTextValue ?? string.Empty;
            ApplyFilter(text);
        }

        // aplica o filtro (case-insensitive, procura em Title, Subtitle e Description)
        private void ApplyFilter(string filter)
        {
            var q = filter.Trim();

            // se vazio -> exibe todos
            IEnumerable<Item> results;
            if (string.IsNullOrWhiteSpace(q))
            {
                results = allItems;
            }
            else
            {
                var low = q.ToLowerInvariant();
                results = allItems.Where(i =>
                    (i.Title?.ToLowerInvariant().Contains(low) ?? false) ||
                    (i.Subtitle?.ToLowerInvariant().Contains(low) ?? false) ||
                    (i.Description?.ToLowerInvariant().Contains(low) ?? false)
                );
            }

            // atualiza ObservableCollection de maneira eficiente (clear + add)
            filteredItems.Clear();
            foreach (var r in results)
                filteredItems.Add(r);

            UpdateCountLabel();
        }

        private void UpdateCountLabel()
        {
            countLabel.Text = $"{filteredItems.Count} itens";
        }

        // seleção do item — navega para DetailPage
        async void ItemsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection == null || e.CurrentSelection.Count == 0)
                return;

            var item = e.CurrentSelection[0] as Item;
            if (item == null)
                return;

            await Navigation.PushAsync(new DetailPage(item));

            // limpa seleção para permitir re-seleção
            ((CollectionView)sender).SelectedItem = null;
        }
       
    }
}

