using LaFrancais.Code;

namespace LaFrancais
{
    public partial class MainPage : ContentPage
    {
        private string EntriesCount => $"{QuizManager.UsedCount} / {QuizManager.Count}";
        private int Submits { get; set; } = 0;
        private int GoodAnswers { get; set; } = 0;
        private int BadAnswers { get; set; } = 0;

        private string GoodAnswersText => $"{((double)GoodAnswers / Submits * 100):0}%";
        private string BadAnswersText => $"{((double)BadAnswers / Submits * 100):0}%";

        private QuizEntry CurrentEntry { get; set; }

        private List<Locale> FrancaisLocates { get; set; } = new List<Locale>();

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
            this.Input_Editor.Focus();
        }

        private async void Confirm_button_Clicked(object sender, EventArgs e)
        {
            var narrator = this.Narrator_Picker.SelectedItem as string;
            if (narrator is not null)
            {
                SpeechOptions speachOptions = new() { Locale = this.FrancaisLocates.Single(l => l.Name == narrator) };
                TextToSpeech.Default.SpeakAsync(this.CurrentEntry.FrancaisSpelling, speachOptions);
            }

            if (this.Input_Editor.Text.Trim().ToLower() == this.CurrentEntry.FrancaisSpelling.Trim().ToLower())
            {
                this.GoodAnswers++;
                await DisplayAlert("Très bien!", $"'{this.CurrentEntry.FrancaisSpelling}' == '{this.CurrentEntry.Meaning}'", "Suivant →");

                this.GoToNextQuestion();
            }
            else
            {
                this.BadAnswers++;
                var next = !await DisplayAlert("Mal!", $"'{this.CurrentEntry.FrancaisSpelling}' == '{this.CurrentEntry.Meaning}'", "Répéter ⭯", "Suivant →");
                if (next)
                {
                    this.GoToNextQuestion();
                }
            }

            this.Submits++;
            this.Good_label.Text = this.GoodAnswersText;
            this.Bad_label.Text = this.BadAnswersText;


            this.Input_Editor.Text = "";
            this.Input_Editor.Focus();
        }

        private void GoToNextQuestion()
        {
            this.CurrentEntry = QuizManager.GetNextEntry()!;
            this.Meaning_Label.Text = this.CurrentEntry?.Meaning;
            this.Count_label.Text = this.EntriesCount;
            this.Image_Image.Source = this.CurrentEntry?.ImageLink;
        }

        private async Task InitialLoad()
        {
            await QuizManager.LoadModules();
            this.PopulateModules();

            this.CurrentEntry = QuizManager.GetNextEntry()!;
            this.Meaning_Label.Text = this.CurrentEntry.Meaning;
            this.Count_label.Text = this.EntriesCount;
            this.Image_Image.Source = this.CurrentEntry.ImageLink;

            this.FrancaisLocates = (await TextToSpeech.GetLocalesAsync()).Where(l => l.Language == "fr-FR").ToList();
            this.Narrator_Picker.ItemsSource = this.FrancaisLocates.Select(l => l.Name).ToList();
            if (this.FrancaisLocates.Count > 0)
            {
                this.Narrator_Picker.SelectedIndex = 0;
            }

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

        private void PopulateModules()
        {
            QuizManager.ActiveModules = QuizManager.Modules!;
            foreach (var module in QuizManager.Modules!)
            {
                var element = new StackLayout() 
                {
                    Spacing = 5,
                    Orientation = StackOrientation.Horizontal
                };
                var label = new Label() { Text = module.Name, VerticalOptions = LayoutOptions.Center, FontSize = 16 };
                var checkbox = new CheckBox() { IsChecked = true };
                checkbox.CheckedChanged += Checkbox_CheckedChanged;

                element.Add(checkbox);
                element.Add(label);

                this.Modules_StackLayout.Add(element);
            }
        }

        private void Checkbox_CheckedChanged(object? sender, CheckedChangedEventArgs e)
        {
            var checkboxList = this.Modules_StackLayout.Children.Cast<StackLayout>().Select(s => s.Children[0] as CheckBox);
            var index = 0;
            for (int i = 0; i < checkboxList.Count(); ++i)
            {
                if ((checkboxList!).ElementAt(i) == (sender! as CheckBox))
                {
                    index = i;
                    break;
                }
            }

            if (e.Value)
            {
                QuizManager.ActiveModules = QuizManager.ActiveModules.Concat(new Module[] { QuizManager.Modules![index] }).ToArray();
            }
            else
            {
                QuizManager.ActiveModules = QuizManager.ActiveModules.Where(m => m != QuizManager.Modules![index]).ToArray();
            }
            this.GoToNextQuestion();
        }

        private void Info_Button_Clicked(object sender, EventArgs e)
        {
            //TODO
        }

        private void Modules_button_Clicked(object sender, EventArgs e)
        {
            this.Modules_Grid.IsVisible = true;
        }

        private void CloseModule_Button_Clicked(object sender, EventArgs e)
        {
            this.Modules_Grid.IsVisible = false;
        }
    }
}