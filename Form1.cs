
using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace bt3
{
    public partial class Form1 : Form
    {
        // Kết nối SQL Server
        private readonly string connectionString =
            @"Server=localhost;Database=MayTinhDB;Integrated Security=True;TrustServerCertificate=True;";

        private double firstNumber = 0;
        private string currentOperator = "";
        private bool isNewNumber = true;

        public Form1()
        {
            InitializeComponent();

            SetupCalculator();

            // Gắn sự kiện các nút số
            btn0.Click += NumberButton_Click;
            btn1.Click += NumberButton_Click;
            btn2.Click += NumberButton_Click;
            btn3.Click += NumberButton_Click;
            btn4.Click += NumberButton_Click;
            btn5.Click += NumberButton_Click;
            btn6.Click += NumberButton_Click;
            btn7.Click += NumberButton_Click;
            btn8.Click += NumberButton_Click;
            btn9.Click += NumberButton_Click;

            // Gắn sự kiện phép tính
            btnPlus.Click += OperatorButton_Click;
            btnMinus.Click += OperatorButton_Click;
            btnMultiply.Click += OperatorButton_Click;
            btnDivide.Click += OperatorButton_Click;

            btnEqual.Click += btnEqual_Click;
            btnClear.Click += btnClear_Click;

            txtDisplay.Text = "0";
        }

        // Sắp xếp giao diện
        private void SetupCalculator()
        {
            ClientSize = new System.Drawing.Size(350, 240);
            Text = "Máy tính";

            txtDisplay.Location = new System.Drawing.Point(12, 12);
            txtDisplay.Size = new System.Drawing.Size(260, 31);
            txtDisplay.TextAlign = HorizontalAlignment.Right;
            txtDisplay.Font = new System.Drawing.Font("Segoe UI", 14);

            btnClear.Location = new System.Drawing.Point(278, 12);

            SetButton(btn7, "7", 12, 50);
            SetButton(btn8, "8", 78, 50);
            SetButton(btn9, "9", 144, 50);

            SetButton(btn4, "4", 12, 86);
            SetButton(btn5, "5", 78, 86);
            SetButton(btn6, "6", 144, 86);

            SetButton(btn1, "1", 12, 122);
            SetButton(btn2, "2", 78, 122);
            SetButton(btn3, "3", 144, 122);

            SetButton(btn0, "0", 12, 158);

            SetButton(btnDivide, "/", 278, 50);
            SetButton(btnMultiply, "*", 278, 86);
            SetButton(btnMinus, "-", 278, 122);
            SetButton(btnPlus, "+", 278, 158);
            SetButton(btnEqual, "=", 278, 194);

            btnClear.Size = new System.Drawing.Size(60, 31);
        }

        private void SetButton(Button button, string text, int x, int y)
        {
            button.Text = text;
            button.Location = new System.Drawing.Point(x, y);
            button.Size = new System.Drawing.Size(60, 30);
        }

        // Nhập số
        private void NumberButton_Click(object? sender, EventArgs e)
        {
            Button button = (Button)sender!;
            string number = button.Text;

            if (isNewNumber || txtDisplay.Text == "0")
            {
                txtDisplay.Text = number;
                isNewNumber = false;
            }
            else
            {
                txtDisplay.Text += number;
            }
        }

        // Chọn phép tính
        private void OperatorButton_Click(object? sender, EventArgs e)
        {
            Button button = (Button)sender!;

            if (!double.TryParse(
                txtDisplay.Text,
                NumberStyles.Float,
                CultureInfo.CurrentCulture,
                out double currentNumber))
            {
                return;
            }

            if (currentOperator != "" && !isNewNumber)
            {
                Calculate();
            }

            firstNumber = double.Parse(
                txtDisplay.Text,
                CultureInfo.CurrentCulture);

            currentOperator = button.Text;
            isNewNumber = true;
        }

        // Lưu lịch sử vào SQL Server
        private void SaveHistory(
            double so1,
            string phepTinh,
            double so2,
            double ketQua)
        {
            string sql = @"
                INSERT INTO dbo.LichSuTinhToan
                (SoThuNhat, PhepTinh, SoThuHai, KetQua)
                VALUES
                (@So1, @PhepTinh, @So2, @KetQua)";

            try
            {
                using (SqlConnection connection =
                    new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command =
                        new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add(
                            "@So1", SqlDbType.Float).Value = so1;

                        command.Parameters.Add(
                            "@PhepTinh", SqlDbType.NVarChar, 10)
                            .Value = phepTinh;

                        command.Parameters.Add(
                            "@So2", SqlDbType.Float).Value = so2;

                        command.Parameters.Add(
                            "@KetQua", SqlDbType.Float).Value = ketQua;

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể lưu lịch sử vào SQL Server.\n" +
                    ex.Message,
                    "Lỗi lưu dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // Thực hiện phép tính
        private void Calculate()
        {
            if (!double.TryParse(
                txtDisplay.Text,
                NumberStyles.Float,
                CultureInfo.CurrentCulture,
                out double secondNumber))
            {
                return;
            }

            double result = 0;

            switch (currentOperator)
            {
                case "+":
                    result = firstNumber + secondNumber;
                    break;

                case "-":
                    result = firstNumber - secondNumber;
                    break;

                case "*":
                    result = firstNumber * secondNumber;
                    break;

                case "/":
                    if (secondNumber == 0)
                    {
                        MessageBox.Show(
                            "Không thể chia cho 0!",
                            "Lỗi",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

                        txtDisplay.Text = "0";
                        currentOperator = "";
                        isNewNumber = true;
                        return;
                    }

                    result = firstNumber / secondNumber;
                    break;

                default:
                    return;
            }

            // Lưu phép tính vào database
            SaveHistory(
                firstNumber,
                currentOperator,
                secondNumber,
                result);

            // Hiển thị kết quả
            txtDisplay.Text = result.ToString(
                "G", CultureInfo.CurrentCulture);

            firstNumber = result;
        }

        // Nút bằng
        private void btnEqual_Click(object? sender, EventArgs e)
        {
            if (currentOperator == "")
                return;

            Calculate();

            currentOperator = "";
            isNewNumber = true;
        }

        // Nút xóa
        private void btnClear_Click(object? sender, EventArgs e)
        {
            txtDisplay.Text = "0";
            firstNumber = 0;
            currentOperator = "";
            isNewNumber = true;
        }
    }
}
