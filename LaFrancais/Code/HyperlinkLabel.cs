﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaFrancais.Code
{
    public class HyperlinkLabel : Label
    {
        public static readonly BindableProperty UrlProperty =
            BindableProperty.Create(nameof(Url), typeof(string), typeof(HyperlinkLabel), null);

        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        public HyperlinkLabel()
        {
            TextColor = Colors.Blue;
            TextDecorations = TextDecorations.Underline;
            GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await Launcher.OpenAsync(Url))
            });
            GestureRecognizers.Add(new ClickGestureRecognizer
            {
                Command = new Command(async () => await Launcher.OpenAsync(Url))
            });
        }
    }
}
