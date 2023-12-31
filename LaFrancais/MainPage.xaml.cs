﻿using CommunityToolkit.Maui.Views;
using LaFrancais.Code;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace LaFrancais
{
    public partial class MainPage : ContentPage
    {
        // Launcher.OpenAsync is provided by Essentials.
        public ICommand UrlCommand => new Command<string>(async (url) => await Launcher.OpenAsync(url));
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
            BindingContext = this;
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
                await DisplayAlert("Very good!", $"'{this.CurrentEntry.FrancaisSpelling}' == '{this.CurrentEntry.Meaning}'", "Next →");

                this.GoToNextQuestion();
            }
            else
            {
                this.BadAnswers++;
                var next = !await DisplayAlert("Bad!", $"'{this.CurrentEntry.FrancaisSpelling}' == '{this.CurrentEntry.Meaning}'", "Try again ⭯", "Next →");
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
            await QuizManager.LoadDictionary();
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
            QuizManager.ActiveModules = QuizManager.AllModules;
            foreach (var chapter in QuizManager.Chapters!)
            {
                var stackForChapter = new StackLayout 
                {
                    Spacing = 5,
                    Orientation = StackOrientation.Vertical
                };
                var stackForExpanderAndButton = new StackLayout
                {
                    Spacing = 15,
                    Orientation = StackOrientation.Horizontal
                };
                var selectChapterButton = new Button
                {
                    Padding = 0,
                    FontFamily = "Segoe Fluent Icons",
                    Text = "\uea98",
                    MinimumWidthRequest = 29,
                    MinimumHeightRequest = 29,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Start,
                    FontSize = 16,
                    Margin = new Thickness(0, 4, 0, 0)
                };
                selectChapterButton.Clicked += SelectChapterButton_Clicked;

                var expander = new Expander 
                {
                    Header = new Label { Text = $" ⮝ {chapter.Name}", FontSize = 24 },
                    Content = stackForChapter,
                    IsExpanded = true
                };
                expander.ExpandedChanged += Expander_ExpandedChanged;

                stackForExpanderAndButton.Add(selectChapterButton);
                stackForExpanderAndButton.Add(expander);

                foreach (var module in chapter.Modules) 
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
                    stackForChapter.Add(element);
                }

                this.Modules_StackLayout.Add(stackForExpanderAndButton);
            }
        }

        private Button[] GetChapterButtonList()
        {
            return this.Modules_StackLayout.Children
                .Cast<StackLayout>()
                .Select(s => s.Children[0] as Button)
                .ToArray()!;
        }

        private int GetChapterIndexFromButton(Button button)
        {
            var buttonList = GetChapterButtonList();
            var index = 0;
            for (int i = 0; i < buttonList.Count(); ++i)
            {
                if ((buttonList!).ElementAt(i) == button)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private void SelectChapterButton_Clicked(object? sender, EventArgs e)
        {
            var realSender = (sender! as Button)!;
            var index = GetChapterIndexFromButton(realSender);
            GetExpanderByChapterIndex(index).IsExpanded = true;
            var checxBoxes = GetModuleCheckBoxes(index);
            if (checxBoxes.All(c => c.IsChecked))
            {
                foreach (var checkbox in checxBoxes)
                {
                    checkbox!.IsChecked = false;
                }
            }
            else
            {
                foreach (var checkbox in checxBoxes)
                {
                    checkbox!.IsChecked = true;
                }
            }
        }

        private void Expander_ExpandedChanged(object? sender, CommunityToolkit.Maui.Core.ExpandedChangedEventArgs e)
        {
            var realSender = (sender! as Expander)!;
            var chapter = (realSender.Header as Label)!.Text.Substring(3);
            if (e.IsExpanded)
            {
                (realSender.Header as Label)!.Text = $" ⮝ {chapter}";
            }
            else
            {
                (realSender.Header as Label)!.Text = $" ⮟ {chapter}";
            }
        }

        private CheckBox[] GetModuleCheckBoxes()
        {
            return this.Modules_StackLayout.Children
                .Cast<StackLayout>()
                .Select(s => s.Children[1] as Expander)
                .Select(s => s.Content as StackLayout)
                .SelectMany(s => s!.Children.Cast<StackLayout>())
                .Select(s => s.Children[0] as CheckBox)
                .ToArray()!;
        }

        private CheckBox[] GetModuleCheckBoxes(int chapterIndex)
        {
            return (GetExpanderByChapterIndex(chapterIndex).Content as StackLayout)!
                .Children.Cast<StackLayout>()
                .Select(s => s.Children[0] as CheckBox)
                .ToArray()!;
        }

        private Expander GetExpanderByChapterIndex(int chapterIndex)
        {
            return this.Modules_StackLayout.Children
                .Cast<StackLayout>()
                .Select(s => s.Children[1] as Expander)
                .ElementAt(chapterIndex)!;
        }

        private int GetModuleIndexFromCheckBoxSender(object sender)
        {
            var checkboxList = GetModuleCheckBoxes();
            var index = 0;
            for (int i = 0; i < checkboxList.Count(); ++i)
            {
                if ((checkboxList!).ElementAt(i) == (sender! as CheckBox))
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private void Checkbox_CheckedChanged(object? sender, CheckedChangedEventArgs e)
        {
            var index = GetModuleIndexFromCheckBoxSender(sender!);   

            if (e.Value)
            {
                QuizManager.ActiveModules = QuizManager.ActiveModules.Concat(new Module[] { QuizManager.AllModules[index] }).ToArray();
            }
            else
            {
                QuizManager.ActiveModules = QuizManager.ActiveModules.Where(m => m != QuizManager.AllModules[index]).ToArray();
            }
            this.GoToNextQuestion();
        }

        private void Info_Button_Clicked(object sender, EventArgs e)
        {
            this.Info_Grid.IsVisible = true;
        }

        private void Modules_button_Clicked(object sender, EventArgs e)
        {
            this.Modules_Grid.IsVisible = true;
        }

        private void CloseModule_Button_Clicked(object sender, EventArgs e)
        {
            this.Modules_Grid.IsVisible = false;
        }

        private void CloseInfo_Button_Clicked(object sender, EventArgs e)
        {
            this.Info_Grid.IsVisible = false;
        }

        private void SelectAll_Button_Clicked(object sender, EventArgs e)
        {
            var checkboxList = GetModuleCheckBoxes();
            if (checkboxList.All(c => c.IsChecked))
            {
                foreach (var checkbox in checkboxList)
                {
                    checkbox!.IsChecked = false;
                }
            }
            else
            {
                foreach (var checkbox in checkboxList)
                {
                    checkbox!.IsChecked = true;
                }
            }
        }
    }
}