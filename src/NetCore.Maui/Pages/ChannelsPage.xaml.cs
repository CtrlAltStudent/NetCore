using NetCore.Maui.Services;

namespace NetCore.Maui.Pages;

public partial class ChannelsPage : ContentPage
{
    private readonly ApiClient _api;

    public ChannelsPage(ApiClient api)
    {
        _api = api;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var channels = await _api.GetFromJsonAsync<List<ChannelDto>>("/api/v1/sales-channels");
            if (channels != null)
                ChannelsList.ItemsSource = channels;
            else
            {
                MessageLabel.Text = "Brak kanałów.";
                MessageLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            MessageLabel.Text = "Błąd: " + ex.Message;
            MessageLabel.IsVisible = true;
        }
    }

    private class ChannelDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
    }
}
