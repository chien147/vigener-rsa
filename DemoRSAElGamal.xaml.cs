using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;

using Microsoft.Win32;
using System.Security.Cryptography;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Collections;



namespace RSA_ELGAMAL
{
    
    public partial class DemoRSAElGamal : Window
    {
        public DemoRSAElGamal()
        {
            InitializeComponent();
            
            //rd_tdRSA.IsChecked = true;
            //rd_tcRSA.IsChecked = false;
            //rsa_maHoaBanRoMoi.IsEnabled = false;            

        }
        #region code mã hóa elgamal
        
        string chuoi = "AÁÀẠẢÃĂẮẰẶẲẴẤẦẬẨẪBCDĐEÉÈẸẺẼÊẾỀỆỂỄGHIÍÌỊỈĨJKLMNOÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠPQRSTUÚÙỦỤŨƯỨỪỰỬỮVWXYÝỲỴỶỸZ~`!@#$%^&*();:?/>.<, ";

        public int[] chuoi_mangchiso(string s)
        {
            int[] mang = new int[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                mang[i] = chuoi.IndexOf(s[i]);
            }
            return mang;
        }

        public string chiso_chuoi(int[] a)
        {

            string s = "";
            for (int i = 0; i < a.Length; i++)
                s += chuoi[a[i]];
            return s;
        }

        public string MaHoa(ref string s, string key)
        {
            string cipherText = "";
            key = key.ToUpper();
            s = s.ToUpper();
            int []p = chuoi_mangchiso(s);
            int []k = chuoi_mangchiso(key);
            int []kq = new int[s.Length] ;


            for(int i = 0, j=0; i < s.Length; i++)
            {
                kq[i] = (p[i] + k[j]) % chuoi.Length;
                j = ++j % key.Length;
            }
                cipherText = chiso_chuoi(kq);
                return cipherText;
        }

        public string GiaiMa(ref string s, string key)
        {   
            key = key.ToUpper();
            s = s.ToUpper();
            string plainText = "";
            int[] c = chuoi_mangchiso(s);
            int[] k = chuoi_mangchiso(key);
            int[] kq = new int[s.Length];
            

            for (int i = 0 , j=0; i < s.Length; i++)
            {
                kq[i] = (c[i] - k[j]) % chuoi.Length;
                if (kq[i] < 0)
                kq[i] = (c[i] + (chuoi.Length - k[j])) % chuoi.Length;
                j = ++j % key.Length;
            }
                plainText = chiso_chuoi(kq);
            return plainText;
        }

        private void btMaHoa_Click(object sender, RoutedEventArgs e)
        {
            string s = txtBanRo.Text;
            string key = So_K.Text;
            string v = MaHoa(ref s, key);
            txt_maHoaBanRo.Text = v.ToString();

        }

        private void btGiaiMa_Click(object sender, RoutedEventArgs e)
        {
            string s = txt_banMaHoaNhanDuoc.Text;
            string key = So_K.Text;
            string v = GiaiMa(ref s, key);
            txt_banGiaima.Text = v.ToString();
        }

        private void btThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion code mã hóa elgamal



        #region Code mã hóa RSA
        private void reset_rsa()
        {
            rsa_soP.Text = rsa_soQ.Text = rsa_soPhiN.Text = rsa_soN.Text = rsa_soE.Text = rsa_soD.Text = string.Empty;

        }

        int RSA_soP, RSA_soQ, RSA_soN, RSA_soE, RSA_soD, RSA_soPhi_n;
        public int RSA_d_dau = 0;

        private int RSA_ChonSoNgauNhien()
        {
            Random rd = new Random();
            return rd.Next(11, 101);// tốc độ chậm nên chọn số bé
        }

        //"Hàm kiểm tra nguyên tố"
        private bool RSA_kiemTraNguyenTo(int xi)
        {
            bool kiemtra = true;
            if (xi == 2 || xi == 3)
            {
                // kiemtra = true;
                return kiemtra;
            }
            else
            {
                if (xi == 1 || xi % 2 == 0 || xi % 3 == 0)
                {
                    kiemtra = false;
                }
                else
                {
                    for (int i = 5; i <= Math.Sqrt(xi); i = i + 6)
                        if (xi % i == 0 || xi % (i + 2) == 0)
                        {
                            kiemtra = false;
                            break;
                        }
                }
            }
            return kiemtra;
        }

        // "Hàm kiểm tra hai số nguyên tố cùng nhau"
        private bool nguyenToCungNhau(int ai, int bi)
        {
            bool ktx_;
            // giải thuật Euclid;
            int temp;
            while (bi != 0)
            {
                temp = ai % bi;
                ai = bi;
                bi = temp;
            }
            if (ai == 1) { ktx_ = true; }
            else ktx_ = false;
            return ktx_;
        }

        // "Hàm lấy mod"
        public int RSA_mod(int mx, int ex, int nx)
        {

            //Sử dụng thuật toán "bình phương nhân"
            //Chuyển e sang hệ nhị phân
            int[] a = new int[100];
            int k = 0;
            do
            {
                a[k] = ex % 2;
                k++;
                ex = ex / 2;
            }
            while (ex != 0);
            //Quá trình lấy dư
            int kq = 1;
            for (int i = k - 1; i >= 0; i--)
            {
                kq = (kq * kq) % nx;
                if (a[i] == 1)
                    kq = (kq * mx) % nx;
            }
            return kq;
        }

        private void RSA_taoKhoa()
        {
            //Tinh n=p*q
            RSA_soN = RSA_soP * RSA_soQ;
            rsa_soN.Text = RSA_soN.ToString();
            //Tính Phi(n)=(p-1)*(q-1)
            RSA_soPhi_n = (RSA_soP - 1) * (RSA_soQ - 1);
            rsa_soPhiN.Text = RSA_soPhi_n.ToString();
            //Tính e là một số ngẫu nhiên có giá trị 0< e <phi(n) và là số nguyên tố cùng nhau với Phi(n)
            do
            {
                Random RSA_rd = new Random();
                RSA_soE = RSA_rd.Next(2, RSA_soPhi_n);
            }
            while (!nguyenToCungNhau(RSA_soE, RSA_soPhi_n));
            rsa_soE.Text = RSA_soE.ToString();

            //Tính d là nghịch đảo modular của e
            RSA_soD = 0;
            int i = 2;
            while (((1 + i * RSA_soPhi_n) % RSA_soE) != 0 || RSA_soD <= 0)
            {
                i++;
                RSA_soD = (1 + i * RSA_soPhi_n) / RSA_soE;
            }
            rsa_soD.Text = RSA_soD.ToString();
        }

        public void RSA_MaHoa(string ChuoiVao)
        {
            // taoKhoa();
            // Chuyen xau thanh ma Unicode
            byte[] mh_temp1 = Encoding.Unicode.GetBytes(ChuoiVao);
            string base64 = Convert.ToBase64String(mh_temp1);

            // Chuyen xau thanh ma Unicode
            int[] mh_temp2 = new int[base64.Length];
            for (int i = 0; i < base64.Length; i++)
            {
                mh_temp2[i] = (int)base64[i];
            }

            //Mảng a chứa các kí tự đã mã hóa
            int[] mh_temp3 = new int[mh_temp2.Length];
            for (int i = 0; i < mh_temp2.Length; i++)
            {
                mh_temp3[i] = RSA_mod(mh_temp2[i], RSA_soE, RSA_soN); // mã hóa
            }

            //Chuyển sang kiểu kí tự trong bảng mã Unicode
            string str = "";
            for (int i = 0; i < mh_temp3.Length; i++)
            {
                str = str + (char)mh_temp3[i];
            }
            byte[] data = Encoding.Unicode.GetBytes(str);
            rsa_BanMaHoa.Text = Convert.ToBase64String(data);
            rsa_banMaHoaGuiDen.Text = Convert.ToBase64String(data);

        }
        // hàm giải mã
        public void RSA_GiaiMa(string ChuoiVao)
        {
            
            byte[] temp2 = Convert.FromBase64String(ChuoiVao);
            string giaima = Encoding.Unicode.GetString(temp2);

            int[] b = new int[giaima.Length];                   // độ dài mảng = giải mã
            for (int i = 0; i < giaima.Length; i++)
            {
                b[i] = (int)giaima[i];
            }
            //Giải mã
            int[] c = new int[b.Length];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = RSA_mod(b[i], RSA_soD, RSA_soN);// giải mã
            }

            string str = "";
            for (int i = 0; i < c.Length; i++)
            {
                str = str + (char)c[i];
            }
            byte[] data2 = Convert.FromBase64String(str);
            rsa_banGiaiMa.Text = Encoding.Unicode.GetString(data2);

        }

        private void rsa_TaoKhoa_Click(object sender, RoutedEventArgs e)
        {

            if (rd_tdRSA.IsChecked == true && rd_tcRSA.IsChecked == false)
            {
                reset_rsa();
                RSA_soP = RSA_soQ = 0;
                do
                {
                    RSA_soP = RSA_ChonSoNgauNhien();
                    RSA_soQ = RSA_ChonSoNgauNhien();
                }
                while (RSA_soP == RSA_soQ || !RSA_kiemTraNguyenTo(RSA_soP) || !RSA_kiemTraNguyenTo(RSA_soQ));
                rsa_soP.Text = RSA_soP.ToString();
                rsa_soQ.Text = RSA_soQ.ToString();
                RSA_taoKhoa();
                RSA_d_dau = 1;
                rsa_btMaHoa.IsEnabled = true;
            }
            else
            {
                if (rd_tdRSA.IsChecked == false && rd_tcRSA.IsChecked == true)
                {
                    if (rsa_soP.Text == "" || rsa_soQ.Text == "")
                        MessageBox.Show("Phải nhập đủ 2 số ", "Thông Báo ", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        RSA_soP = int.Parse(rsa_soP.Text);
                        RSA_soQ = int.Parse(rsa_soQ.Text);
                        if (RSA_soP == RSA_soQ)
                        {
                            MessageBox.Show("Nhập 2 số nguyên tố khác nhau ", " Thông Báo", MessageBoxButton.OK, MessageBoxImage.Error);
                            rsa_soQ.Focus();
                        }
                        else
                        {
                            if (!RSA_kiemTraNguyenTo(RSA_soP) || RSA_soP <= 1)
                            {
                                MessageBox.Show("Phải nhập số nguyên  tố [p] lớn hơn 1 ", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Error);
                                rsa_soP.Focus();
                            }
                            else
                            {
                                if (!RSA_kiemTraNguyenTo(RSA_soQ) || RSA_soQ <= 1)
                                {
                                    MessageBox.Show("Phải nhập số nguyên  tố [q] lớn hơn 1 ", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Error);
                                    rsa_soQ.Focus();
                                }
                                else
                                {
                                    RSA_taoKhoa();
                                    rsa_soP.Text = RSA_soP.ToString();
                                    rsa_soQ.Text = RSA_soQ.ToString();
                                    RSA_d_dau = 1;
                                    
                                }
                            }
                        }
                    }

                }
            }
        }

        private void rsa_btMaHoa_Click(object sender, RoutedEventArgs e)
        {
            if (RSA_d_dau != 1)
            { MessageBox.Show("Bạn chưa tạo khóa!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Information); }

            else
            {
                if (rsa_BanRo.Text == "")
                {
                    MessageBox.Show("Bạn chưa nhập bản rõ để mã hóa!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                else
                {
                    // thực hiện mã hóa
                    try
                    {
                        RSA_MaHoa(rsa_BanRo.Text);
                        //rsa_btMaHoa.IsEnabled = false;
                        //rsa_btGiaiMa.IsEnabled = true;
                        //RSA_d_dau = 2;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void rsa_btGiaiMa_Click(object sender, RoutedEventArgs e)
        {

            if (RSA_d_dau != 1)
                MessageBox.Show("Bạn phải tạo khóa trước ", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                try
                {
                    RSA_GiaiMa(rsa_banMaHoaGuiDen.Text);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            //RSA_d_dau = 1;
            //rsa_btGiaiMa.IsEnabled = false;
            //rsa_maHoaBanRoMoi.IsEnabled = true;
        }


        private void rd_tdRSA_Checked(object sender, RoutedEventArgs e)
        {
            rsa_TaoKhoa.IsEnabled = true;
            rsa_soP.Text = rsa_soQ.Text = rsa_soPhiN.Text = rsa_soN.Text = rsa_soE.Text = rsa_soD.Text = string.Empty;
            rsa_soP.IsEnabled = rsa_soQ.IsEnabled = rsa_soPhiN.IsEnabled = rsa_soN.IsEnabled = rsa_soE.IsEnabled = rsa_soD.IsEnabled = false;

        }

        private void rd_tcRSA_Checked(object sender, RoutedEventArgs e)
        {
            rsa_TaoKhoa.IsEnabled = true;
            rsa_soP.Text = rsa_soQ.Text = rsa_soPhiN.Text = rsa_soN.Text = rsa_soE.Text = rsa_soD.Text = string.Empty;
            rsa_soP.IsEnabled = rsa_soQ.IsEnabled = rsa_soPhiN.IsEnabled = rsa_soN.IsEnabled = rsa_soE.IsEnabled = rsa_soD.IsEnabled = true;
        }


        private void rsa_maHoaBanRoMoi_Click(object sender, RoutedEventArgs e)
        {
            rsa_btMaHoa.IsEnabled = true;
            rsa_BanRo.Text = rsa_BanMaHoa.Text = rsa_banMaHoaGuiDen.Text = rsa_banGiaiMa.Text = string.Empty;
            RSA_d_dau = 1;
        }

        private void rsa_btThoat_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void rsa_TaoKhoaMoi_Click(object sender, RoutedEventArgs e)
        {
            
            RSA_d_dau = 0;
            rsa_TaoKhoa.IsEnabled = true;
            rd_tdRSA.IsEnabled = true;
            rd_tdRSA.IsChecked = true;
            rd_tcRSA.IsEnabled = true;
            rd_tcRSA.IsChecked = false;
            rsa_soP.Text = rsa_soQ.Text = rsa_soPhiN.Text = rsa_soN.Text = rsa_soE.Text = rsa_soD.Text = string.Empty;
            rsa_banGiaiMa.Text = rsa_BanMaHoa.Text = rsa_BanRo.Text = rsa_banMaHoaGuiDen.Text = string.Empty;

        }

        private void rsa_soP_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text != "." && IsNumber(e.Text) == false)
            {
                e.Handled = true;
            }
            else if (e.Text == ".")
            {
                if (((TextBox)sender).Text.IndexOf(e.Text) > -1)
                {
                    e.Handled = true;
                }
            }
        }

        private bool IsNumber(string Text_x)
        {
            int outPut;
            return int.TryParse(Text_x, out outPut);
        }
        #endregion
       
    }
}



