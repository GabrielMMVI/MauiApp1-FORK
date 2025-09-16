using Microsoft.Maui.Controls;
using MauiApp1.Models;

namespace MauiApp1.Pages
{
    public partial class DetailPage : ContentPage
    {
        public Item Item { get; }

        public DetailPage(Item item)
        {
            InitializeComponent();
            Item = item;
            BindingContext = item; // Liga o item aos bindings da tela
        }

        async void BackButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
