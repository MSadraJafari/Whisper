using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Devices.InfoPages
{
    public partial class CountrySelectorWindow : Window
    {
        private readonly List<Country> _allCountries;
        public event EventHandler<Country> CountrySelected;

        public CountrySelectorWindow(List<Country> countries, Country currentCountry = null)
        {
            InitializeComponent();
            _allCountries = countries.OrderBy(c => c.Name).ToList();

            Loaded += CountrySelectorWindow_Loaded;
        }

        private void CountrySelectorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCountries(_allCountries);
            txtSearch.Focus();
        }

        private void LoadCountries(IEnumerable<Country> countries)
        {
            lstCountries.Items.Clear();

            foreach (var country in countries)
            {
                var item = new ListBoxItem
                {
                    Content = CreateCountryItem(country),
                    Tag = country,
                    Padding = new Thickness(4),
                    Margin = new Thickness(0, 2, 0, 2)
                };
                lstCountries.Items.Add(item);
            }
        }

        private StackPanel CreateCountryItem(Country country)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var flag = new TextBlock
            {
                Text = country.Flag,
                FontSize = 26,
                Width = 42,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Noto Color Emoji"),  // مهم‌ترین خط
                Foreground = Brushes.White
            };

            var name = new TextBlock
            {
                Text = country.Name,
                FontSize = 15,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(14, 0, 0, 0)
            };

            var code = new TextBlock
            {
                Text = country.Code,
                FontSize = 15,
                Foreground = Brushes.LightBlue,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(22, 0, 0, 0),
                FontWeight = FontWeights.SemiBold
            };

            panel.Children.Add(flag);
            panel.Children.Add(name);
            panel.Children.Add(code);

            return panel;
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(query))
            {
                LoadCountries(_allCountries);
            }
            else
            {
                var filtered = _allCountries.Where(c =>
                    c.Name.ToLower().Contains(query) ||
                    c.Code.Contains(query));

                LoadCountries(filtered);
            }
        }

        private void lstCountries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstCountries.SelectedItem is ListBoxItem item && item.Tag is Country selected)
            {
                CountrySelected?.Invoke(this, selected);
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void txtSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}