using DANHSACHSINHVIEN.Entities; // Make sure this namespace matches your entities
using System;
using System.Data.Entity; // Required for Entity Framework
using System.Linq;
using System.Windows.Forms;

namespace DANHSACHSINHVIEN
{
    public partial class Form1 : Form
    {
        private bool isEditing; // Flag to check if we are editing

        public Form1()
        {
            InitializeComponent(); // Initialize the form components
        }

        // Event handler for form load
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadDanhSachSinhVien(); // Load the student list
            LoadDanhSachLop(); // Load the class list into ComboBox
            SetButtonState(true); // Set the initial button state
        }

        // Method to load the student list
        private void LoadDanhSachSinhVien()
        {
            using (var context = new Model1())
            {
                var sinhVienList = context.SinhViens.Include(s => s.Lop).ToList();
                lvSinhvien.Items.Clear(); // Clear the ListView items
                foreach (var sinhVien in sinhVienList)
                {
                    ListViewItem item = new ListViewItem(sinhVien.MaSV);
                    item.SubItems.Add(sinhVien.HoTenSV);
                    item.SubItems.Add(String.Format("{0:dd/MM/yyyy}", sinhVien.NgaySinh)); // Format DateTime
                    item.SubItems.Add(sinhVien.Lop?.TenLop ?? ""); // Display class name if available
                    lvSinhvien.Items.Add(item); // Add item to ListView
                }
            }
        }
        private void SearchStudents(string searchTerm)
        {
            using (var context = new Model1())
            {
                var sinhVienList = context.SinhViens
                    .Include(s => s.Lop)
                    .Where(sv => sv.HoTenSV.Contains(searchTerm) || sv.MaSV.Contains(searchTerm))
                    .ToList();

                lvSinhvien.Items.Clear(); // Clear existing items
                foreach (var sinhVien in sinhVienList)
                {
                    ListViewItem item = new ListViewItem(sinhVien.MaSV);
                    item.SubItems.Add(sinhVien.HoTenSV);
                    item.SubItems.Add(String.Format("{0:dd/MM/yyyy}", sinhVien.NgaySinh)); // Format DateTime
                    item.SubItems.Add(sinhVien.Lop?.TenLop ?? ""); // Display class name if available
                    lvSinhvien.Items.Add(item); // Add item to ListView
                }
            }
        }
        // Method to load the class list into ComboBox
        private void LoadDanhSachLop()
        {
            using (var context = new Model1())
            {
                var lopList = context.Lops.ToList(); // Get class list from the database
                cboLop.DataSource = lopList; // Set the data source for ComboBox
                cboLop.DisplayMember = "TenLop"; // Display class name
                cboLop.ValueMember = "MaLop"; // Set value member to class ID
            }
        }

        // Method to set the button states (Add, Edit, Delete, Save, Cancel)
        private void SetButtonState(bool isDefaultState)
        {
            btThem.Enabled = isDefaultState;
            btSua.Enabled = isDefaultState;
            btXoa.Enabled = isDefaultState;
            btLuu.Enabled = !isDefaultState;
            btKhong.Enabled = !isDefaultState;
        }

        // Method to clear input fields
        private void ClearInputFields()
        {
            txtMaSV.Clear();
            txtHotenSV.Clear();
            dtNgaysinh.Value = DateTime.Now;
            cboLop.SelectedIndex = -1; // Reset ComboBox selection
        }

        // Event handler for Add Student button

        // Event handler for ListView item selection
        private void lvSinhvien_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvSinhvien.SelectedItems.Count > 0)
            {
                // Get the selected student from ListView
                var selectedItem = lvSinhvien.SelectedItems[0];

                // Get data from ListView columns and display in input fields
                txtMaSV.Text = selectedItem.SubItems[0].Text;
                txtHotenSV.Text = selectedItem.SubItems[1].Text;

                // Handle displaying Date of Birth, ensuring correct date format
                DateTime parsedDate;
                if (DateTime.TryParseExact(selectedItem.SubItems[2].Text, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out parsedDate))
                {
                    dtNgaysinh.Value = parsedDate; // Set the value for DateTimePicker
                }
                else
                {
                    MessageBox.Show("Lỗi định dạng ngày tháng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Find class in ComboBox based on class name in ListView column
                var tenLop = selectedItem.SubItems[3].Text;
                if (!string.IsNullOrEmpty(tenLop))
                {
                    cboLop.SelectedIndex = cboLop.FindStringExact(tenLop); // Select the correct class
                }
                else
                {
                    cboLop.SelectedIndex = -1; // No class selected if none exists
                }

                // Enable edit and delete buttons when a student is selected
                SetButtonState(false); // Allow editing and deleting
            }
        }

        // Event handler for Delete button
       

        // Event handler for Edit button
        private void btSua_Click(object sender, EventArgs e)
        {
            if (lvSinhvien.SelectedItems.Count > 0)
            {
                var selectedItem = lvSinhvien.SelectedItems[0];

                // Populate input fields with the selected student's details
                txtMaSV.Text = selectedItem.SubItems[0].Text;
                txtHotenSV.Text = selectedItem.SubItems[1].Text;

                DateTime parsedDate;
                if (DateTime.TryParseExact(selectedItem.SubItems[2].Text, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out parsedDate))
                {
                    dtNgaysinh.Value = parsedDate; // Set the Date of Birth
                }

                cboLop.Text = selectedItem.SubItems[3].Text; // Set the class name

                // Mark that we are now editing an existing student
                isEditing = true;

                // Enable input fields and disable other buttons to avoid conflicts
                SetButtonState(false);
                txtMaSV.Enabled = false; // MaSV should not be editable in "Edit" mode
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một sinh viên để sửa!", "Thông báo");
            }
        }

        // Event handler for Exit button
        private void btThoat_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn thoát không?", "Thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Close(); // Close the form
            }
        }

        // Event handler for Cancel button
        private void btKhong_Click(object sender, EventArgs e)
        {
            ClearInputFields();
            SetButtonState(true); // Reset button states

            // Reset editing mode
            isEditing = false;
            txtMaSV.Enabled = true; // Re-enable MaSV input field
        }

        private void btThem_Click_1(object sender, EventArgs e)
        {
            ClearInputFields();
            SetButtonState(false);
            txtMaSV.Focus(); // Focus on the MaSV input field
        }

        private void btLuu_Click_1(object sender, EventArgs e)
        {
            using (var context = new Model1())
            {
                if (string.IsNullOrEmpty(txtMaSV.Text) || cboLop.SelectedValue == null)
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ thông tin sinh viên!", "Lỗi");
                    return; // Exit if required fields are missing
                }

                if (isEditing)
                {
                    // Edit an existing student
                    var maSV = txtMaSV.Text;
                    var sinhVien = context.SinhViens.FirstOrDefault(sv => sv.MaSV == maSV);

                    if (sinhVien != null)
                    {
                        sinhVien.HoTenSV = txtHotenSV.Text;
                        sinhVien.NgaySinh = dtNgaysinh.Value;
                        sinhVien.MaLop = cboLop.SelectedValue.ToString();

                        // Save changes to the database
                        context.SaveChanges();
                        MessageBox.Show("Cập nhật sinh viên thành công!", "Thông báo");
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy sinh viên để cập nhật!", "Thông báo");
                    }
                }
                else
                {
                    // Add a new student
                    var sinhVien = new SinhVien
                    {
                        MaSV = txtMaSV.Text,
                        HoTenSV = txtHotenSV.Text,
                        NgaySinh = dtNgaysinh.Value,
                        MaLop = cboLop.SelectedValue?.ToString()
                    };

                    context.SinhViens.Add(sinhVien);
                    context.SaveChanges();
                    MessageBox.Show("Thêm sinh viên thành công!", "Thông báo");
                }

                // Reload the student list after adding/editing
                LoadDanhSachSinhVien();
            }

            // Clear fields and reset form
            ClearInputFields();
            SetButtonState(true);
            isEditing = false; // Reset editing mode
            txtMaSV.Enabled = true; // Enable MaSV input field
        }

        private void btXoa_Click_1(object sender, EventArgs e)
        {
            if (lvSinhvien.SelectedItems.Count > 0)
            {
                var selectedItem = lvSinhvien.SelectedItems[0];
                var maSV = selectedItem.SubItems[0].Text;

                using (var context = new Model1())
                {
                    var sinhVien = context.SinhViens.FirstOrDefault(sv => sv.MaSV == maSV);
                    if (sinhVien != null)
                    {
                        context.SinhViens.Remove(sinhVien);
                        context.SaveChanges();
                        LoadDanhSachSinhVien();
                        MessageBox.Show("Xóa sinh viên thành công!", "Thông báo");
                    }
                }

                ClearInputFields();
                SetButtonState(true); // Reset button states
            }
        }

        private void btSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                SearchStudents(searchTerm);
            }
            else
            {
                LoadDanhSachSinhVien(); // Reload all students if search term is empty
            }
        }
    }
}
