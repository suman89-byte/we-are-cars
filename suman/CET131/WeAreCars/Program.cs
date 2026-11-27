

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace WeAreCars
{
    
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SplashForm());
        }
    }

    // 1. SPLASH SCREEN

    public class SplashForm : Form
    {
        private readonly Timer _timer;

        public SplashForm()
        {
            Text = "WeAreCars - Welcome";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.FromArgb(30, 30, 60);
            ClientSize = new Size(540, 340);
            Cursor = Cursors.Hand;

            var lblTitle = new Label
            {
                Text = "WeAreCars",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(150, 40)
            };

            var lblSub = new Label
            {
                Text = "Car Rental Booking System",
                Font = new Font("Segoe UI", 13),
                ForeColor = Color.LightSteelBlue,
                AutoSize = true,
                Location = new Point(160, 100)
            };

            var lblInfo = new Label
            {
                Text = "Welcome to the WeAreCars booking system.\r\n\r\n" +
                       "1. Log in with your staff credentials.\r\n" +
                       "2. Enter the customer and rental details.\r\n" +
                       "3. Check the summary, then confirm to store the booking.\r\n\r\n" +
                       "Existing bookings can be edited or deleted from the main screen.\r\n" +
                       "Fields marked * are mandatory.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gainsboro,
                AutoSize = false,
                Size = new Size(460, 160),
                Location = new Point(45, 140)
            };

            var lblHint = new Label
            {
                Text = "(click anywhere to continue)",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(195, 305)
            };

            Controls.AddRange(new Control[] { lblTitle, lblSub, lblInfo, lblHint });

            _timer = new Timer { Interval = 3000 };
            _timer.Tick += (s, e) => OpenLogin();
            _timer.Start();

            Click += (s, e) => OpenLogin();
        }

        
        private void OpenLogin()
        {
            _timer.Stop();
            Hide();

            bool continueLoop = true;
            while (continueLoop)
            {
                using (var login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK)
                        break;

                    using (var main = new MainForm(login.LoggedInUser))
                    {
                        main.ShowDialog();
                        continueLoop = main.LogoutRequested;
                    }
                }
            }

            Close();
        }
    }

    // 2. LOGIN SCREEN
   

    public class LoginForm : Form
    {
        private const string ValidUsername = "sta001";
        private const string ValidPassword = "givemethekeys123";

        private readonly TextBox _txtUsername;
        private readonly TextBox _txtPassword;
        private readonly Label _lblError;

        public string LoggedInUser { get; private set; }

        public LoginForm()
        {
            Text = "WeAreCars - Staff Login";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(380, 250);
            BackColor = Color.WhiteSmoke;

            var lblHeading = new Label
            {
                Text = "Staff Login",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 60),
                AutoSize = true,
                Location = new Point(115, 18)
            };

            var lblUser = new Label
            {
                Text = "Username:",
                AutoSize = true,
                Location = new Point(40, 75),
                Font = new Font("Segoe UI", 9.5f)
            };
            _txtUsername = new TextBox
            {
                Location = new Point(150, 72),
                Width = 190,
                Font = new Font("Segoe UI", 10)
            };

            var lblPass = new Label
            {
                Text = "Password:",
                AutoSize = true,
                Location = new Point(40, 112),
                Font = new Font("Segoe UI", 9.5f)
            };
            _txtPassword = new TextBox
            {
                Location = new Point(150, 109),
                Width = 190,
                UseSystemPasswordChar = true,
                Font = new Font("Segoe UI", 10)
            };

            _lblError = new Label
            {
                Text = "",
                ForeColor = Color.Firebrick,
                AutoSize = true,
                Location = new Point(40, 148),
                MaximumSize = new Size(310, 0)
            };

            var btnLogin = new Button
            {
                Text = "Sign In",
                Location = new Point(150, 178),
                Width = 100,
                Height = 32,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            var tips = new ToolTip();
            tips.SetToolTip(_txtPassword, "Enter the staff password provided by your line manager.");

            Controls.AddRange(new Control[]
            {
                lblHeading, lblUser, _txtUsername, lblPass, _txtPassword, _lblError, btnLogin
            });

            AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            if (_txtUsername.Text.Trim() == ValidUsername &&
                _txtPassword.Text == ValidPassword)
            {
                LoggedInUser = _txtUsername.Text.Trim();
                DialogResult = DialogResult.OK;   // closes the login form
            }
            else
            {
                _lblError.Text = "Username or password is incorrect.";
                _txtPassword.Clear();
                _txtPassword.Focus();
            }
        }
    }

    // 3. MAIN BOOKING SCREEN
    public class MainForm : Form
    {
      
        public bool LogoutRequested { get; private set; }
        private readonly string _staffUsername;
        private List<Booking> _bookings = new List<Booking>();
        private Booking _editingBooking = null;   // null = new booking mode

     
        private TextBox _txtFirstName;
        private TextBox _txtSurname;
        private TextBox _txtAddress;
        private NumericUpDown _numAge;
        private CheckBox _chkValidLicense;

        
        private NumericUpDown _numDays;
        private ComboBox _cmbCarType;
        private ComboBox _cmbFuelType;
        private CheckBox _chkUnlimitedMileage;
        private CheckBox _chkBreakdownCover;
        private Label _lblTotal;

      
        private Button _btnBook;
        private Button _btnClear;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnRefresh;
        private DataGridView _gridBookings;
        private Label _lblMode;

        private ErrorProvider _errors;
        private ToolTip _tips;

        private static readonly string[] CarTypes = { "City", "Family", "Sports", "SUV" };
        private static readonly string[] FuelTypes = { "Petrol", "Diesel", "Hybrid", "Electric" };

        public MainForm(string staffUsername)
        {
            _staffUsername = string.IsNullOrWhiteSpace(staffUsername) ? "sta001" : staffUsername;

            Text = "WeAreCars - Booking System";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1220, 770);
            MinimumSize = new Size(1000, 700);
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9.5f);

            _errors = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
            _tips = new ToolTip();

            BuildHeader();
            BuildPersonalPanel();
            BuildBookingPanel();
            BuildBookingsPanel();

            LoadBookings();
            UpdateTotal();
        }


        private void BuildHeader()
        {
            var header = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(ClientSize.Width, 64),
                BackColor = Color.FromArgb(30, 30, 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblTitle = new Label
            {
                Text = "WeAreCars  —  Car Rental Booking System",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 18)
            };

            var lblUser = new Label
            {
                Text = $"Signed in: {_staffUsername}",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.LightSteelBlue,
                AutoSize = true
            };

            var btnLogout = new Button
            {
                Text = "Log Out",
                BackColor = Color.Firebrick,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(96, 32),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => BtnLogout_Click();

            header.Controls.AddRange(new Control[] { lblTitle, lblUser, btnLogout });

           
            header.Resize += (s, e) =>
            {
                btnLogout.Location = new Point(header.ClientSize.Width - btnLogout.Width - 20, 16);
                lblUser.Location = new Point(
                    header.ClientSize.Width - btnLogout.Width - lblUser.Width - 30, 22);
            };

            Controls.Add(header);
        }

      
        private void BuildPersonalPanel()
        {
            var grp = new GroupBox
            {
                Text = "  Personal Information",
                Location = new Point(10, 74),
                Size = new Size(560, 300),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 60),
                BackColor = Color.White
            };

            int labelX = 20, inputX = 190, inputWidth = 340;
            int y = 42, dy = 42;

            grp.Controls.Add(MakeLabel("First Name *", labelX, y));
            _txtFirstName = new TextBox { Location = new Point(inputX, y - 3), Width = inputWidth };
            _tips.SetToolTip(_txtFirstName, "Enter the customer's first name (letters only).");
            grp.Controls.Add(_txtFirstName);
            y += dy;

            grp.Controls.Add(MakeLabel("Surname *", labelX, y));
            _txtSurname = new TextBox { Location = new Point(inputX, y - 3), Width = inputWidth };
            _tips.SetToolTip(_txtSurname, "Enter the customer's surname.");
            grp.Controls.Add(_txtSurname);
            y += dy;

            grp.Controls.Add(MakeLabel("Address *", labelX, y));
            _txtAddress = new TextBox { Location = new Point(inputX, y - 3), Width = inputWidth };
            _tips.SetToolTip(_txtAddress, "Enter the customer's full home address.");
            grp.Controls.Add(_txtAddress);
            y += dy;

            grp.Controls.Add(MakeLabel("Age *", labelX, y));
            _numAge = new NumericUpDown
            {
                Location = new Point(inputX, y - 3),
                Width = 90,
                Minimum = 16,
                Maximum = 120,
                Value = 18
            };
            _tips.SetToolTip(_numAge, "Driver must be between 18 and 120 years old.");
            grp.Controls.Add(_numAge);
            y += dy;

            grp.Controls.Add(MakeLabel("Valid driving licence? *", labelX, y));
            _chkValidLicense = new CheckBox
            {
                Text = "Customer holds a valid driving licence",
                Location = new Point(inputX, y - 3),
                AutoSize = true
            };
            _tips.SetToolTip(_chkValidLicense, "Bookings cannot proceed without a valid driving licence.");
            grp.Controls.Add(_chkValidLicense);

            Controls.Add(grp);
        }

       
        private void BuildBookingPanel()
        {
            var grp = new GroupBox
            {
                Text = "  Booking Information",
                Location = new Point(10, 384),
                Size = new Size(560, 376),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 60),
                BackColor = Color.White
            };

            int labelX = 20, inputX = 190;
            int y = 38, dy = 40;

            grp.Controls.Add(MakeLabel("Number of days *", labelX, y));
            _numDays = new NumericUpDown
            {
                Location = new Point(inputX, y - 3),
                Width = 90,
                Minimum = 1,
                Maximum = 28
            };
            _tips.SetToolTip(_numDays, "Rental duration must be between 1 and 28 days.");
            grp.Controls.Add(_numDays);
            y += dy;

            grp.Controls.Add(MakeLabel("Type of car *", labelX, y));
            _cmbCarType = new ComboBox
            {
                Location = new Point(inputX, y - 3),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbCarType.Items.AddRange(CarTypes);
            _cmbCarType.SelectedIndex = 0;
            _tips.SetToolTip(_cmbCarType, "City: free. Family: +£50. Sports: +£75. SUV: +£65.");
            grp.Controls.Add(_cmbCarType);
            y += dy;

            grp.Controls.Add(MakeLabel("Fuel type *", labelX, y));
            _cmbFuelType = new ComboBox
            {
                Location = new Point(inputX, y - 3),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbFuelType.Items.AddRange(FuelTypes);
            _cmbFuelType.SelectedIndex = 0;
            _tips.SetToolTip(_cmbFuelType, "Petrol/Diesel: free. Hybrid: +£30. Electric: +£50.");
            grp.Controls.Add(_cmbFuelType);
            y += dy;

            grp.Controls.Add(MakeLabel("Optional extras", labelX, y));
            _chkUnlimitedMileage = new CheckBox
            {
                Text = "Unlimited mileage (+£10/day)",
                Location = new Point(inputX, y - 3),
                AutoSize = true
            };
            grp.Controls.Add(_chkUnlimitedMileage);
            y += 30;

            _chkBreakdownCover = new CheckBox
            {
                Text = "Breakdown cover (+£2/day)",
                Location = new Point(inputX, y - 3),
                AutoSize = true
            };
            grp.Controls.Add(_chkBreakdownCover);
            y += 42;

     
            var lblTotalCaption = new Label
            {
                Text = "Total price:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 60),
                AutoSize = true,
                Location = new Point(labelX, y + 8)
            };
            _lblTotal = new Label
            {
                Text = "£0.00",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.SeaGreen,
                AutoSize = true,
                Location = new Point(inputX, y)
            };
            grp.Controls.Add(lblTotalCaption);
            grp.Controls.Add(_lblTotal);
            y += 58;

           
            _btnBook = new Button
            {
                Text = "Book",
                Location = new Point(labelX, y),
                Size = new Size(140, 40),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnBook.FlatAppearance.BorderSize = 0;
            _btnBook.Click += BtnBook_Click;

            _btnClear = new Button
            {
                Text = "Clear Form",
                Location = new Point(170, y),
                Size = new Size(140, 40),
                BackColor = Color.LightSlateGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnClear.FlatAppearance.BorderSize = 0;
            _btnClear.Click += (s, e) => ClearForm();

            grp.Controls.AddRange(new Control[] { _btnBook, _btnClear });

           
            _lblMode = new Label
            {
                Text = "",
                ForeColor = Color.DarkOrange,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(320, y + 12)
            };
            grp.Controls.Add(_lblMode);

            Controls.Add(grp);

            
            foreach (var c in new Control[] { _numDays, _cmbCarType, _cmbFuelType,
                                              _chkUnlimitedMileage, _chkBreakdownCover })
                HookTotalRecalc(c);
        }

      
        private void BuildBookingsPanel()
        {
            var grp = new GroupBox
            {
                Text = "  Current Bookings",
                Location = new Point(580, 74),
                Size = new Size(630, 686),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 60),
                BackColor = Color.White
            };

            _gridBookings = new DataGridView
            {
                Location = new Point(12, 30),
                Size = new Size(606, 560),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ColumnHeadersHeight = 34,
                EnableHeadersVisualStyles = false,
                AutoGenerateColumns = false
            };
            _gridBookings.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 60);
            _gridBookings.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _gridBookings.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _gridBookings.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            _gridBookings.DefaultCellStyle.SelectionBackColor = Color.SteelBlue;
            _gridBookings.DefaultCellStyle.SelectionForeColor = Color.White;
            _gridBookings.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);

          
            _gridBookings.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "BookedAt", HeaderText = "Booked At", FillWeight = 110 });
            _gridBookings.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "FirstName", HeaderText = "First Name", FillWeight = 80 });
            _gridBookings.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "Surname", HeaderText = "Surname", FillWeight = 80 });
            _gridBookings.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "Days", HeaderText = "Days", FillWeight = 50 });
            _gridBookings.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "CarType", HeaderText = "Car", FillWeight = 70 });
            _gridBookings.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "FuelType", HeaderText = "Fuel", FillWeight = 80 });
            _gridBookings.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Total",
                HeaderText = "Total (£)",
                FillWeight = 80,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

         
            _btnEdit = MakeActionButton("Edit Selected", Color.SteelBlue, 12);
            _btnEdit.Click += BtnEdit_Click;

            _btnDelete = MakeActionButton("Delete Selected", Color.Firebrick, 148);
            _btnDelete.Click += BtnDelete_Click;

            _btnRefresh = MakeActionButton("Refresh", Color.DarkSlateGray, 284);
            _btnRefresh.Click += (s, e) => LoadBookings();

            _btnEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _btnRefresh.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            foreach (var b in new[] { _btnEdit, _btnDelete, _btnRefresh })
            {
                b.Top = 605;
                _tips.SetToolTip(b, b.Text);
            }

            grp.Controls.AddRange(new Control[] { _gridBookings, _btnEdit, _btnDelete, _btnRefresh });
            Controls.Add(grp);
        }

       
        private static Label MakeLabel(string text, int x, int y) =>
            new Label
            {
                Text = text,
                AutoSize = true,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 9.5f)
            };

       
        private static Button MakeActionButton(string text, Color back, int x) =>
            new Button
            {
                Text = text,
                Location = new Point(x, 605),
                Size = new Size(128, 40),
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

     

        private void HookTotalRecalc(Control c)
        {
            switch (c)
            {
                case NumericUpDown n: n.ValueChanged += (s, e) => UpdateTotal(); break;
                case ComboBox cb: cb.SelectedIndexChanged += (s, e) => UpdateTotal(); break;
                case CheckBox chk: chk.CheckedChanged += (s, e) => UpdateTotal(); break;
            }
        }

        private void UpdateTotal()
        {
            decimal total = PriceCalculator.Calculate(
                (int)_numDays.Value,
                _cmbCarType.Text,
                _cmbFuelType.Text,
                _chkUnlimitedMileage.Checked,
                _chkBreakdownCover.Checked);
            _lblTotal.Text = $"£{total:0.00}";
        }

       
        private void LoadBookings()
        {
            _bookings = BookingStore.Load();
            _bookings.Reverse();   // newest first
            _gridBookings.DataSource = null;
            _gridBookings.DataSource = _bookings;
            _gridBookings.ClearSelection();
        }

       
        private void ClearForm()
        {
            _txtFirstName.Clear();
            _txtSurname.Clear();
            _txtAddress.Clear();
            _numAge.Value = 18;
            _chkValidLicense.Checked = false;
            _numDays.Value = 1;
            _cmbCarType.SelectedIndex = 0;
            _cmbFuelType.SelectedIndex = 0;
            _chkUnlimitedMileage.Checked = false;
            _chkBreakdownCover.Checked = false;
            _errors.Clear();

            _editingBooking = null;
            _btnBook.Text = "Book";
            _btnBook.BackColor = Color.SeaGreen;
            _lblMode.Text = "";

            UpdateTotal();
        }

       
        private List<string> ValidateForm()
        {
            var errors = new List<string>();
            _errors.Clear();

            if (string.IsNullOrWhiteSpace(_txtFirstName.Text))
            { errors.Add("Enter the customer's first name."); _errors.SetError(_txtFirstName, "Required"); }

            if (string.IsNullOrWhiteSpace(_txtSurname.Text))
            { errors.Add("Enter the customer's surname."); _errors.SetError(_txtSurname, "Required"); }

            if (string.IsNullOrWhiteSpace(_txtAddress.Text))
            { errors.Add("Enter the customer's address."); _errors.SetError(_txtAddress, "Required"); }

            if (_numAge.Value < 18 || _numAge.Value > 120)
            { errors.Add("Age must be between 18 and 120."); _errors.SetError(_numAge, "18-120 only"); }

            if (!_chkValidLicense.Checked)
            { errors.Add("A valid driving licence is required."); _errors.SetError(_chkValidLicense, "Licence required"); }

            if (_numDays.Value < 1 || _numDays.Value > 28)
            { errors.Add("Rental duration must be from 1 to 28 days."); _errors.SetError(_numDays, "1-28 only"); }

            if (!CarTypes.Contains(_cmbCarType.Text))
            { errors.Add("Choose a valid car type."); _errors.SetError(_cmbCarType, "Choose a car"); }

            if (!FuelTypes.Contains(_cmbFuelType.Text))
            { errors.Add("Choose a valid fuel type."); _errors.SetError(_cmbFuelType, "Choose a fuel"); }

            return errors;
        }

        
        private Booking BuildBookingFromForm()
        {
            bool isEdit = _editingBooking != null;

            return new Booking
            {
                Id = isEdit ? _editingBooking.Id : Guid.NewGuid().ToString(),
                BookedAt = isEdit
                    ? _editingBooking.BookedAt
                    : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                FirstName = _txtFirstName.Text.Trim(),
                Surname = _txtSurname.Text.Trim(),
                Address = _txtAddress.Text.Trim(),
                Age = (int)_numAge.Value,
                ValidLicense = _chkValidLicense.Checked,
                Days = (int)_numDays.Value,
                CarType = _cmbCarType.Text,
                FuelType = _cmbFuelType.Text,
                UnlimitedMileage = _chkUnlimitedMileage.Checked,
                BreakdownCover = _chkBreakdownCover.Checked,
                Total = PriceCalculator.Calculate(
                    (int)_numDays.Value, _cmbCarType.Text, _cmbFuelType.Text,
                    _chkUnlimitedMileage.Checked, _chkBreakdownCover.Checked)
            };
        }

        // ---- Book / Save changes ---------------------------------------------

        private void BtnBook_Click(object sender, EventArgs e)
        {
            var errors = ValidateForm();
            if (errors.Count > 0)
            {
                MessageBox.Show(
                    "Please fix the following before booking:\r\n\r\n- " +
                    string.Join("\r\n- ", errors),
                    "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isEdit = _editingBooking != null;
            var booking = BuildBookingFromForm();

            using (var summary = new SummaryForm(booking, isEdit))
            {
                if (summary.ShowDialog() == DialogResult.OK)
                {
                    if (isEdit)
                    {
                        BookingStore.Update(booking);
                        MessageBox.Show("Booking updated successfully.", "WeAreCars",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        BookingStore.Save(booking);
                        MessageBox.Show("Booking stored successfully.", "WeAreCars",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    LoadBookings();
                    ClearForm();
                }
            }
        }

        // ---- Edit selected ---------------------------------------------------

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedBooking();
            if (selected == null)
            {
                MessageBox.Show("Please select a booking in the list to edit.",
                    "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _editingBooking = selected;

          
            _txtFirstName.Text = selected.FirstName;
            _txtSurname.Text = selected.Surname;
            _txtAddress.Text = selected.Address;
            _numAge.Value = Math.Max(_numAge.Minimum, Math.Min(_numAge.Maximum, selected.Age));
            _chkValidLicense.Checked = selected.ValidLicense;
            _numDays.Value = Math.Max(_numDays.Minimum, Math.Min(_numDays.Maximum, selected.Days));
            _cmbCarType.SelectedItem = CarTypes.Contains(selected.CarType) ? selected.CarType : CarTypes[0];
            _cmbFuelType.SelectedItem = FuelTypes.Contains(selected.FuelType) ? selected.FuelType : FuelTypes[0];
            _chkUnlimitedMileage.Checked = selected.UnlimitedMileage;
            _chkBreakdownCover.Checked = selected.BreakdownCover;
            _errors.Clear();
            UpdateTotal();

            _btnBook.Text = "Save Changes";
            _btnBook.BackColor = Color.DarkOrange;
            _lblMode.Text = "Editing existing booking";
        }

        // ---- Delete selected -------------------------------------------------

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedBooking();
            if (selected == null)
            {
                MessageBox.Show("Please select a booking in the list to delete.",
                    "No selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Delete booking for {selected.FirstName} {selected.Surname} " +
                $"({selected.CarType} / {selected.FuelType}, {selected.Days} day(s))?\r\n\r\n" +
                "This cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            BookingStore.Delete(selected.Id);

          
            if (_editingBooking != null && _editingBooking.Id == selected.Id)
                ClearForm();

            LoadBookings();
            MessageBox.Show("Booking deleted.", "WeAreCars",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ---- Logout ----------------------------------------------------------

        private void BtnLogout_Click()
        {
            var confirm = MessageBox.Show(
                "Log out of WeAreCars?\r\nAny unsaved changes on this form will be lost.",
                "Confirm Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            LogoutRequested = true;
            Close();  
        }

        

        private Booking GetSelectedBooking()
        {
            if (_gridBookings.CurrentRow == null) return null;
            return _gridBookings.CurrentRow.DataBoundItem as Booking;
        }
    }

    // 4. SUMMARY CONFIRMATION SCREEN
    public class SummaryForm : Form
    {
        public SummaryForm(Booking b, bool isEdit)
        {
            Text = isEdit ? "Save Changes - Please Confirm" : "Booking Summary - Please Confirm";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(500, 520);
            BackColor = Color.WhiteSmoke;

            var lblHeading = new Label
            {
                Text = isEdit ? "Confirm Changes" : "Confirm Booking",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 60),
                AutoSize = true,
                Location = new Point(150, 15)
            };

            var grpDetails = new GroupBox
            {
                Text = "  Booking details",
                Location = new Point(20, 55),
                Size = new Size(460, 320),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 60)
            };

            var lblDetails = new Label
            {
                Text =
                    $"Customer : {b.FirstName} {b.Surname}\r\n" +
                    $"Address  : {b.Address}\r\n" +
                    $"Age      : {b.Age}      Licence : {(b.ValidLicense ? "Valid" : "None")}\r\n" +
                    $"Duration : {b.Days} day(s)\r\n" +
                    $"Car      : {b.CarType}\r\n" +
                    $"Fuel     : {b.FuelType}\r\n" +
                    $"Extras   : {ExtrasText(b)}",
                Font = new Font("Consolas", 10.5f),
                AutoSize = false,
                Size = new Size(430, 180),
                Location = new Point(15, 35)
            };

            var lblTotalCaption = new Label
            {
                Text = "TOTAL PRICE:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 60),
                AutoSize = true,
                Location = new Point(15, 245)
            };

            var lblTotal = new Label
            {
                Text = $"£{b.Total:0.00}",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.SeaGreen,
                AutoSize = true,
                Location = new Point(190, 235)
            };

            grpDetails.Controls.AddRange(new Control[] { lblDetails, lblTotalCaption, lblTotal });

            var btnConfirm = new Button
            {
                Text = isEdit ? "Save Changes" : "Confirm Booking",
                DialogResult = DialogResult.OK,
                Location = new Point(80, 420),
                Size = new Size(160, 45),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnConfirm.FlatAppearance.BorderSize = 0;

            var btnBack = new Button
            {
                Text = "Go Back",
                DialogResult = DialogResult.Cancel,
                Location = new Point(260, 420),
                Size = new Size(160, 45),
                BackColor = Color.LightSlateGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnBack.FlatAppearance.BorderSize = 0;

            Controls.AddRange(new Control[] { lblHeading, grpDetails, btnConfirm, btnBack });
            AcceptButton = btnConfirm;
            CancelButton = btnBack;
        }

       
        private static string ExtrasText(Booking b)
        {
            var parts = new List<string>();
            if (b.UnlimitedMileage) parts.Add("Unlimited mileage");
            if (b.BreakdownCover) parts.Add("Breakdown cover");
            return parts.Count == 0 ? "None" : string.Join(", ", parts);
        }
    }

    // 5. BOOKING DATA RECORD
    public class Booking
    {
        public string Id { get; set; }                // unique identifier (GUID)
        public string BookedAt { get; set; }          // when the booking was made
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string Address { get; set; }
        public int Age { get; set; }
        public bool ValidLicense { get; set; }
        public int Days { get; set; }
        public string CarType { get; set; }
        public string FuelType { get; set; }
        public bool UnlimitedMileage { get; set; }
        public bool BreakdownCover { get; set; }
        public decimal Total { get; set; }
    }

    // 6. BOOKING STORAGE (Bookings.txt)
    public static class BookingStore
    {
        private const string Header = "WeAreCars bookings (JSON Lines)";

        private static readonly string FilePath =
            System.IO.Path.Combine(Application.StartupPath, "Bookings.txt");

        
        public static List<Booking> Load()
        {
            var bookings = new List<Booking>();
            if (!File.Exists(FilePath)) return bookings;

            bool needsRewrite = false;

            foreach (var line in File.ReadLines(FilePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var b = JsonSerializer.Deserialize<Booking>(line);
                    if (b != null && !string.IsNullOrWhiteSpace(b.FirstName))
                    {
                        if (string.IsNullOrEmpty(b.Id))
                        {
                            b.Id = Guid.NewGuid().ToString();
                            needsRewrite = true;
                        }
                        bookings.Add(b);
                    }
                }
                catch (JsonException)
                {
                   
                }
            }

            if (needsRewrite) RewriteAll(bookings);
            return bookings;
        }

         public static void Save(Booking booking)
        {
            try
            {
                if (string.IsNullOrEmpty(booking.Id))
                    booking.Id = Guid.NewGuid().ToString();

                bool needsHeader = !File.Exists(FilePath) || new FileInfo(FilePath).Length == 0;
                using (var writer = new StreamWriter(FilePath, append: true))
                {
                    if (needsHeader) writer.WriteLine(Header);
                    writer.WriteLine(JsonSerializer.Serialize(booking));
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show("Booking could not be saved. Check the folder is writable.\r\n" + ex.Message,
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

     
        public static void Update(Booking updated)
        {
            var all = Load();
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].Id == updated.Id)
                {
                    all[i] = updated;
                    break;
                }
            }
            RewriteAll(all);
        }

      
        public static void Delete(string id)
        {
            var all = Load();
            all.RemoveAll(b => b.Id == id);
            RewriteAll(all);
        }

       
        private static void RewriteAll(List<Booking> bookings)
        {
            try
            {
                using (var writer = new StreamWriter(FilePath, append: false))
                {
                    writer.WriteLine(Header);
                    foreach (var b in bookings)
                        writer.WriteLine(JsonSerializer.Serialize(b));
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show("Bookings could not be saved. Check the folder is writable.\r\n" + ex.Message,
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // 7. PRICING RULES
    public static class PriceCalculator
    {
        public const decimal BaseRatePerDay = 25m;
        public const decimal UnlimitedMileagePerDay = 10m;
        public const decimal BreakdownCoverPerDay = 2m;

        public static decimal Calculate(int days, string carType, string fuelType,
            bool unlimitedMileage, bool breakdownCover)
        {
            decimal total = BaseRatePerDay * days;

            total += carType switch
            {
                "Family" => 50m,
                "Sports" => 75m,
                "SUV" => 65m,
                _ => 0m
            };

            total += fuelType switch
            {
                "Hybrid" => 30m,
                "Electric" => 50m,
                _ => 0m
            };

            if (unlimitedMileage) total += UnlimitedMileagePerDay * days;
            if (breakdownCover) total += BreakdownCoverPerDay * days;

            return total;
        }
    }
}
