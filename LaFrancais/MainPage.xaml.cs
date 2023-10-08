using LaFrancais.Code;

namespace LaFrancais
{
    public partial class MainPage : ContentPage
    {
        private string EntriesCount => $"{QuizManager.UsedCount} / {QuizManager.Count}";
        private int Submits { get; set; } = 0;
        private int GoodAnswers { get; set; } = 0;
        private int BadAnswers { get; set; } = 0;

        private string GoodAnswersText => $"{(double)GoodAnswers / Submits * 100}%";
        private string BadAnswersText => $"{(double)BadAnswers / Submits * 100}%";

        private QuizEntry CurrentEntry { get; set; }

        public MainPage()
        {
            InitializeComponent();
            this.Loading_ProgressBar.ProgressTo(1, 3000, Easing.CubicIn);
            this.InitialLoad();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var senderButton = sender as Button;
            this.Input_Editor.Text += senderButton!.Text;
        }

        private async void Confirm_button_Clicked(object sender, EventArgs e)
        {
            IEnumerable<Locale> locales = await TextToSpeech.GetLocalesAsync();
            var francaisLocate = locales.FirstOrDefault(l => l.Country == "");

            TextToSpeech.Default.SpeakAsync(this.CurrentEntry.FrancaisSpelling);

            if (this.Input_Editor.Text.Trim().ToLower() == this.CurrentEntry.FrancaisSpelling.Trim().ToLower())
            {
                this.GoodAnswers++;
                await DisplayAlert("Très bien!", $"'{this.CurrentEntry.FrancaisSpelling}' == '{this.CurrentEntry.Meaning}'", "Suivant");

                this.CurrentEntry = (await QuizManager.GetNextEntry())!;
                this.Meaning_Label.Text = this.CurrentEntry.Meaning;
                this.Count_label.Text = this.EntriesCount;
                this.Image_Image.Source = this.CurrentEntry.ImageLink;
            }
            else
            {
                this.BadAnswers++;
                await DisplayAlert("Mal!", $"'{this.CurrentEntry.FrancaisSpelling}' == '{this.CurrentEntry.Meaning}'", "Suivant");
            }

            this.Submits++;
            this.Good_label.Text = this.GoodAnswersText;
            this.Bad_label.Text = this.BadAnswersText;


            this.Input_Editor.Text = "";
            this.Input_Editor.Focus();
        }

        private async Task InitialLoad()
        {
            this.CurrentEntry = (await QuizManager.GetNextEntry())!;
            this.Meaning_Label.Text = this.CurrentEntry.Meaning;
            this.Count_label.Text = this.EntriesCount;
            this.Image_Image.Source = this.CurrentEntry.ImageLink;

            this.Loading_Grid.IsVisible = false;

            this.Input_Editor.Focus();
        }

        private void Input_Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue is not null && e.NewTextValue.Any() && (e.NewTextValue.Last() == '\n' || e.NewTextValue.Last() == '\r'))
            {
                this.Confirm_button_Clicked(this.Confirm_button, null);
            }
        }
    }
}