using LaFrancais.Code;

namespace LaFrancais
{
    public partial class MainPage : ContentPage
    {
        private string EntriesCount => $"{QuizManager.UsedCount} / {QuizManager.Count}";

        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var senderButton = sender as Button;
            this.Input_Editor.Text += senderButton!.Text;
        }

        private async void Confirm_button_Clicked(object sender, EventArgs e)
        {
            this.Meaning_Label.Text = (await QuizManager.GetNextEntry())!.Meaning;
            this.Count_label.Text = this.EntriesCount;
        }
    }
}