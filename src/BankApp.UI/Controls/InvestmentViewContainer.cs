using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Data;
using BankApp.Core.Interfaces;
using BankApp.UI.Forms;

namespace BankApp.UI.Controls
{
    /// <summary>
    /// Main Investment container - manages navigation between MarketHome and InstrumentDetail views
    /// TradingView-like navigation flow
    /// </summary>
    public class InvestmentViewContainer : XtraUserControl
    {
        private readonly IMarketDataProvider _stockProvider;
        private readonly IMarketDataProvider _cryptoProvider;
        private readonly TransactionService _transactionService;
        private readonly IAccountRepository _accountRepository;
        
        // Navigation
        private PanelControl contentPanel;
        private MarketHomeView _marketHomeView;
        private InstrumentDetailView _instrumentDetailView;
        
        private string _currentView = "Home"; // "Home" or "Detail"
        private string _currentSymbol;
        
        public InvestmentViewContainer()
        {
            // Initialize services
            _stockProvider = new FinnhubMarketDataProvider();
            _cryptoProvider = new BinanceMarketDataProvider();
            
            var context = new DapperContext();
            _accountRepository = new AccountRepository(context);
            var transactionRepo = new TransactionRepository(context);
            var auditRepo = new AuditRepository(context);
            _transactionService = new TransactionService(_accountRepository, transactionRepo, auditRepo);
            
            InitializeComponents();
            this.Load += InvestmentViewContainer_Load;
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(14, 14, 14);
            this.Padding = new Padding(0);
            this.Margin = new Padding(0);
            
            contentPanel = new PanelControl();
            contentPanel.Name = "contentPanel";
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            contentPanel.Appearance.BackColor = Color.FromArgb(14, 14, 14);
            contentPanel.Visible = true;
            contentPanel.Padding = new Padding(0);
            
            this.Controls.Add(contentPanel);
            contentPanel.BringToFront();
        }

        private void InvestmentViewContainer_Load(object sender, EventArgs e)
        {
            ShowMarketHome();
        }

        /// <summary>
        /// Show market home view (list of stocks/crypto)
        /// </summary>
        private void ShowMarketHome()
        {
            contentPanel.SuspendLayout();
            contentPanel.Controls.Clear();
            
            if (_marketHomeView == null)
            {
                _marketHomeView = new MarketHomeView(_stockProvider, _cryptoProvider);
                _marketHomeView.AssetSelected += MarketHomeView_AssetSelected;
                _marketHomeView.TradeTerminalRequested += OnTradeTerminalRequested;
            }
            
            _marketHomeView.Dock = DockStyle.Fill;
            _marketHomeView.Visible = true;
            contentPanel.Controls.Add(_marketHomeView);
            _marketHomeView.BringToFront();
            contentPanel.ResumeLayout(true);
            _currentView = "Home";
        }

        /// <summary>
        /// Show instrument detail view (chart + order ticket)
        /// </summary>
        private void ShowInstrumentDetail(string symbol)
        {
            contentPanel.SuspendLayout();
            contentPanel.Controls.Clear();
            
            _currentSymbol = symbol;
            
            // Determine which provider to use based on symbol
            var provider = symbol.EndsWith("USDT") ? _cryptoProvider : _stockProvider;
            
            if (_instrumentDetailView == null)
            {
                _instrumentDetailView = new InstrumentDetailView(provider);
                _instrumentDetailView.BackRequested += InstrumentDetailView_BackRequested;
                _instrumentDetailView.TradeTerminalRequested += OnTradeTerminalRequested;
                _instrumentDetailView.Dock = DockStyle.Fill;
            }
            
            _instrumentDetailView.LoadSymbol(symbol);
            contentPanel.Controls.Add(_instrumentDetailView);
            _instrumentDetailView.BringToFront();
            contentPanel.ResumeLayout(true);
            _currentView = "Detail";
        }

        private void MarketHomeView_AssetSelected(object sender, string symbol)
        {
            ShowInstrumentDetail(symbol);
        }
        
        private void InstrumentDetailView_BackRequested(object sender, EventArgs e)
        {
            ShowMarketHome();
        }
        
        private void OnTradeTerminalRequested(object sender, string symbol)
        {
            var form = new TradeTerminalForm();
            form.Show();
        }

        /// <summary>
        /// Navigate back to market home
        /// </summary>
        public void NavigateBack()
        {
            if (_currentView == "Detail")
            {
                ShowMarketHome();
            }
        }

        /// <summary>
        /// Refresh current view
        /// </summary>
        public async void Refresh()
        {
            if (_currentView == "Home" && _marketHomeView != null)
            {
                await _marketHomeView.RefreshAsync();
            }
        }
    }
}
