namespace EventProcessing
{
    public class MyMessage
    {
        public string Name { get; set; }
        public string Email { get; set; }

        public string Content { get; set; }

        public override string ToString ()
        {
            return $"{Name} : {Email}";
        }
    }
}