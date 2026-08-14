
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskManager
{
    public partial class Form1 : Form
    {
        // =====================================================
        // FIREBASE CONNECTION
        // =====================================================

        private readonly FirebaseClient firebaseClient =
            new FirebaseClient(
                "https://nagrajtaskmanager-default-rtdb.firebaseio.com/"
            );


        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public Form1()
        {
            InitializeComponent();

            this.Load += Form1_Load;
        }


        // =====================================================
        // FORM LOAD
        // =====================================================

        private async void Form1_Load(object sender, EventArgs e)
        {
            SetupDataGridView();

            await LoadTasks();
        }


        // =====================================================
        // SETUP DATAGRIDVIEW
        // =====================================================

        private void SetupDataGridView()
        {
            dgvTasks.Columns.Clear();

            dgvTasks.Columns.Add(
                "Id",
                "ID"
            );

            dgvTasks.Columns.Add(
                "TaskName",
                "Task Name"
            );

            dgvTasks.Columns.Add(
                "Description",
                "Description"
            );

            dgvTasks.Columns.Add(
                "DueDate",
                "Due Date"
            );

            dgvTasks.Columns.Add(
                "Completed",
                "Status"
            );

            // Make columns wider
            dgvTasks.Columns["Id"].Width = 120;
            dgvTasks.Columns["TaskName"].Width = 180;
            dgvTasks.Columns["Description"].Width = 300;
            dgvTasks.Columns["DueDate"].Width = 120;
            dgvTasks.Columns["Completed"].Width = 120;

            // Full row selection
            dgvTasks.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvTasks.MultiSelect = false;

            dgvTasks.ReadOnly = true;

            dgvTasks.AllowUserToAddRows = false;
        }


        // =====================================================
        // ADD TASK
        // =====================================================

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // ---------------------------------------------
                // Validate Task Name
                // ---------------------------------------------

                if (string.IsNullOrWhiteSpace(txtTask.Text))
                {
                    MessageBox.Show(
                        "Please enter a task.",
                        "Validation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    txtTask.Focus();

                    return;
                }


                // ---------------------------------------------
                // Validate Description
                // ---------------------------------------------

                if (string.IsNullOrWhiteSpace(txtDescription.Text))
                {
                    MessageBox.Show(
                        "Please enter a description.",
                        "Validation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    txtDescription.Focus();

                    return;
                }


                // ---------------------------------------------
                // Create Task Object
                // ---------------------------------------------

                TaskModel task = new TaskModel
                {
                    TaskName = txtTask.Text.Trim(),

                    Description = txtDescription.Text.Trim(),

                    DueDate = dtpDate.Value.Date,

                    Completed = false,

                    CreatedAt = DateTime.Now
                };


                // ---------------------------------------------
                // Save to Firebase
                // ---------------------------------------------

                await firebaseClient
                    .Child("Tasks")
                    .PostAsync(task);


                // ---------------------------------------------
                // Success Message
                // ---------------------------------------------

                MessageBox.Show(
                    "Task added successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );


                // ---------------------------------------------
                // Clear Fields
                // ---------------------------------------------

                txtTask.Clear();

                txtDescription.Clear();

                dtpDate.Value = DateTime.Today;


                // ---------------------------------------------
                // Reload Tasks
                // ---------------------------------------------

                await LoadTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error adding task:\n\n" +
                    ex.Message,
                    "Firebase Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        // =====================================================
        // LOAD TASKS
        // =====================================================

        private async Task LoadTasks()
        {
            try
            {
                // ---------------------------------------------
                // Get Tasks From Firebase
                // ---------------------------------------------

                var firebaseTasks =
                    await firebaseClient
                    .Child("Tasks")
                    .OnceAsync<TaskModel>();


                // ---------------------------------------------
                // Clear DataGridView
                // ---------------------------------------------

                dgvTasks.Rows.Clear();


                // ---------------------------------------------
                // Display Tasks
                // ---------------------------------------------

                foreach (var item in firebaseTasks)
                {
                    string status =
                        item.Object.Completed
                        ? "Completed"
                        : "Pending";


                    dgvTasks.Rows.Add(
                        item.Key,
                        item.Object.TaskName,
                        item.Object.Description,
                        item.Object.DueDate.ToShortDateString(),
                        status
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error loading tasks:\n\n" +
                    ex.Message,
                    "Firebase Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        // =====================================================
        // DELETE TASK
        // =====================================================

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // ---------------------------------------------
                // Check Selected Row
                // ---------------------------------------------

                if (dgvTasks.SelectedRows.Count == 0)
                {
                    MessageBox.Show(
                        "Please select a task.",
                        "Select Task",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }


                // ---------------------------------------------
                // Get Firebase Task ID
                // ---------------------------------------------

                string taskId =
                    dgvTasks
                    .SelectedRows[0]
                    .Cells["Id"]
                    .Value
                    .ToString();


                // ---------------------------------------------
                // Confirm Delete
                // ---------------------------------------------

                DialogResult result =
                    MessageBox.Show(
                        "Are you sure you want to delete this task?",
                        "Delete Task",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );


                if (result != DialogResult.Yes)
                {
                    return;
                }


                // ---------------------------------------------
                // Delete From Firebase
                // ---------------------------------------------

                await firebaseClient
                    .Child("Tasks")
                    .Child(taskId)
                    .DeleteAsync();


                // ---------------------------------------------
                // Success Message
                // ---------------------------------------------

                MessageBox.Show(
                    "Task deleted successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );


                // ---------------------------------------------
                // Reload
                // ---------------------------------------------

                await LoadTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error deleting task:\n\n" +
                    ex.Message,
                    "Firebase Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        // =====================================================
        // COMPLETE TASK
        // =====================================================

        private async void btnComplete_Click(object sender, EventArgs e)
        {
            try
            {
                // ---------------------------------------------
                // Check Selected Row
                // ---------------------------------------------

                if (dgvTasks.SelectedRows.Count == 0)
                {
                    MessageBox.Show(
                        "Please select a task.",
                        "Select Task",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }


                // ---------------------------------------------
                // Get Firebase ID
                // ---------------------------------------------

                string taskId =
                    dgvTasks
                    .SelectedRows[0]
                    .Cells["Id"]
                    .Value
                    .ToString();


                // ---------------------------------------------
                // Get Task
                // ---------------------------------------------

                var task =
                    await firebaseClient
                    .Child("Tasks")
                    .Child(taskId)
                    .OnceSingleAsync<TaskModel>();


                if (task == null)
                {
                    MessageBox.Show(
                        "Task not found.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );

                    return;
                }


                // ---------------------------------------------
                // Mark Task Completed
                // ---------------------------------------------

                task.Completed = true;


                // ---------------------------------------------
                // Update Firebase
                // ---------------------------------------------

                await firebaseClient
                    .Child("Tasks")
                    .Child(taskId)
                    .PutAsync(task);


                // ---------------------------------------------
                // Success
                // ---------------------------------------------

                MessageBox.Show(
                    "Task marked as completed!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );


                // ---------------------------------------------
                // Reload
                // ---------------------------------------------

                await LoadTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error completing task:\n\n" +
                    ex.Message,
                    "Firebase Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        // =====================================================
        // REFRESH
        // =====================================================

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadTasks();
        }

        private void pnlInputArea_Paint(object sender, PaintEventArgs e)
        {

        }
    }


    // =========================================================
    // FIREBASE TASK MODEL
    // =========================================================

    public class TaskModel
    {
        // Task name
        public string TaskName { get; set; } = "";


        // Task description
        public string Description { get; set; } = "";


        // Task due date
        public DateTime DueDate { get; set; }


        // Task status
        public bool Completed { get; set; }


        // Task creation date
        public DateTime CreatedAt { get; set; }
    }
}

