using System.Globalization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Accessibility;
using Microsoft.Maui.Controls;

namespace MauiApp1;

public partial class MainPage : ContentPage
{
    private readonly ISpeechToText _speechToTextService;
    private int count = 0;
    public SpeechToTextState? State => _speechToTextService.CurrentState;

    public MainPage([FromKeyedServices("Online")] ISpeechToText speechToTextService)
    {
        InitializeComponent();

        _speechToTextService = speechToTextService;
        _speechToTextService.StateChanged += HandleSpeechToTextStateChanged;
        _speechToTextService.RecognitionResultCompleted += HandleRecognitionResultCompleted;
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }

    private async void OnStartSpeechToTextClicked(object sender, EventArgs e)
    {
        var isGranted = await _speechToTextService.RequestPermissions(CancellationToken.None).ConfigureAwait(false);
        if (!isGranted)
        {
            await Toast.Make("Permission not granted").Show(CancellationToken.None).ConfigureAwait(false);
            return;
        }

        _speechToTextService.RecognitionResultUpdated += HandleRecognitionResultUpdated;

        try
        {
            await _speechToTextService.StartListenAsync(
                new SpeechToTextOptions()
                {
                    // Culture = CultureInfo.GetCultureInfo("PL"),
                    Culture = CultureInfo.GetCultureInfo("pl-PL"),
                    ShouldReportPartialResults = true,
                },
                CancellationToken.None
            );
        }
        catch (Exception ex)
        {
            SpeechResultLabel.Text = $"Error: {ex.Message}";
        }
    }

        private void HandleRecognitionResultUpdated(
            object? sender,
            SpeechToTextRecognitionResultUpdatedEventArgs e
        )
        {
            SpeechResultLabel.Text += e.RecognitionResult;
        }

        private void HandleSpeechToTextStateChanged(
            object? sender,
            SpeechToTextStateChangedEventArgs e
        )
        {
            OnPropertyChanged(nameof(State));
        }

        private void HandleRecognitionResultCompleted(
            object? sender,
            SpeechToTextRecognitionResultCompletedEventArgs e
        )
        {
            SpeechResultLabel.Text = e.RecognitionResult.IsSuccessful
                ? e.RecognitionResult.Text
                : e.RecognitionResult.Exception.Message;
        }
}