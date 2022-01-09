namespace Terminal.Models
{
    public class PrivilegeItem
    {
        public string Privilege { get; set; }
        public bool Granted { get; set; }
        public bool Checked { get; set; }

        public PrivilegeItem(string privilege)
        {
            Privilege = privilege;
        }

        public void GrantPermission(bool value)
        {
            Granted = value;
            Checked = true;
        }
    }
}
