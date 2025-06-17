using BeWithMe.Models;

namespace BeWithMe.DTOs
{
    public class retrievePostsDTO
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public AuthorDTO Author { get; set; }
        public int ReactionsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool isAccepted { get; set; }
        //public List<AcceptorDTO> Acceptors { get; set; }

    }
}
