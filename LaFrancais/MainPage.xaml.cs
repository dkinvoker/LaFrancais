using LaFrancais.Code;

namespace LaFrancais
{
    public partial class MainPage : ContentPage
    {
        private string EntriesCount => $"{QuizManager.UsedCount} / {QuizManager.Count}";
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
            if (this.Input_Editor.Text.ToLower() == this.CurrentEntry.FrancaisSpelling.ToLower())
            {
                await DisplayAlert("Très bien!", $"'{this.CurrentEntry.FrancaisSpelling}' == '{this.CurrentEntry.Meaning}'", "suivant");

                this.CurrentEntry = (await QuizManager.GetNextEntry())!;
                this.Meaning_Label.Text = this.CurrentEntry.Meaning;
                this.Count_label.Text = this.EntriesCount;
                this.Image_Image.Source = this.CurrentEntry.ImageLink;
            }
            else
            {
                await DisplayAlert("Mal!", $"'{this.CurrentEntry.FrancaisSpelling}' == '{this.CurrentEntry.Meaning}'", "suivant");
            }  

            this.Input_Editor.Focus();
        }

        private async Task InitialLoad()
        {
            this.CurrentEntry = (await QuizManager.GetNextEntry())!;
            this.Meaning_Label.Text = this.CurrentEntry.Meaning;
            this.Count_label.Text = this.EntriesCount;
            this.Image_Image.Source = this.CurrentEntry.ImageLink;

            this.Loading_Grid.IsVisible = false;
        }
    }
}