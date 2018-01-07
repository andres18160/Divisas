using DivisasMVVM.Classes;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DivisasMVVM.ViewModels
{
    public class MainViewModel: INotifyPropertyChanged
    {
        #region Atributos
        private decimal amount;
        private double sourceRate;
        private double targetRate;
        private bool isEnabled;
        private bool isRunning;
        private string message;
        private ExchangeRates exchangeRates;

        #endregion

        #region Propiedades
        public ObservableCollection<Rate> Rates { get; set; }

        public string Message
        {
            set
            {
                if (message != value)
                {
                    message = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
                }
            }
            get
            {
                return message;
            }
        }

        public decimal Amount
        {
            set
            {
                if(amount != value)
                {
                    amount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Amount)));
                }
            }
            get
            {
                return amount;
            }
        }

        public double SourceRate
        {
            set
            {
                if (sourceRate != value)
                {
                    sourceRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SourceRate)));
                }
            }
            get
            {
                return sourceRate;
            }
        }

        public double TargetRate
        {
            set
            {
                if (targetRate != value)
                {
                    targetRate = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetRate)));
                }
            }
            get
            {
                return targetRate;
            }
        }

        public bool IsRunning
        {
            set
            {
                if (isRunning != value)
                {
                    isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
                }
            }
            get
            {
                return isRunning;
            }
        }

        public bool IsEnabled
        {
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
                }
            }
            get
            {
                return isEnabled;
            }
        }
        #endregion
        
        #region Comandos
        public ICommand ConvertCommand { get { return new RelayCommand(ConvertMoney); } }
        public ICommand SwithcCommand { get { return new RelayCommand(Swithc); } }

        private void Swithc()
        {
            var aux = SourceRate;
            SourceRate = TargetRate;
            TargetRate = aux;
            ConvertMoney();
        }

        private async void ConvertMoney()
        {
            if (Amount <= 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must enter an amount", "Acept");
                return;
            }

            if (SourceRate == 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must select a source rate", "Acept");
                return;
            }

            if (TargetRate == -1)
            {
                await App.Current.MainPage.DisplayAlert("Error", "You must select a target rate", "Acept");
                return;
            }

            decimal amountConverted = amount / (decimal)sourceRate * (decimal)targetRate;

            Message = string.Format("{0:N2} = {1:N2}", amount, amountConverted);
        }

        #endregion

        #region Eventos
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructores
        public MainViewModel()
        {
            Rates = new ObservableCollection<Rate>();
            Message = "Enter an amount, select a source currency, select a target currency and press Convert button";
            IsEnabled = false;
            IsRunning = false;
            LoadRate();

        }


        #endregion

        #region Metodos
        private async void LoadRate()
        {
            IsRunning = true;
            try
            {
                var client = new HttpClient();
                client.BaseAddress =new Uri("https://openexchangerates.org");
                var url = "/api/latest.json?app_id=f490efbcd52d48ee98fd62cf33c47b9e";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Message = response.StatusCode.ToString();
                    IsRunning = false;
                    return;
                }

                var result = await response.Content.ReadAsStringAsync();
                exchangeRates = JsonConvert.DeserializeObject<ExchangeRates>(result);

            }
            catch (Exception ex)
            {
                Message = ex.Message;
                IsRunning = false;
                return;
            }
            ConvertRates();

            IsRunning = false;
            IsEnabled = true;

        }

        private void ConvertRates()
        {
            Rates.Clear();
            var type = typeof(Rates);
            var properties = type.GetRuntimeFields();

            foreach (var property in properties)
            {
                var code = property.Name.Substring(1, 3);
                Rates.Add(new Rate
                {
                    Code = code,
                    TaxRate = (double)property.GetValue(exchangeRates.Rates),
                });
            }

        }
        #endregion

    }
}
