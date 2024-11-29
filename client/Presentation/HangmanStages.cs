namespace HangmanClient.Presentation
{
    public static class HangmanStages
    {
        public static readonly string[] Stages = new[]
        {
            @"
     ═╦════╦══
      ║    │
      ║    
      ║
      ║
      ║
      ║
    ╔═║═╗
",
            @"
     ═╦════╦══
      ║    │
      ║    O
      ║
      ║
      ║
      ║
    ╔═║═╗
",
            @"
     ═╦════╦══
      ║    │
      ║    O
      ║    │
      ║
      ║
      ║
    ╔═║═╗
",
            @"
     ═╦════╦══
      ║    │
      ║    O
      ║   /│
      ║
      ║
      ║
    ╔═║═╗
",
            @"
     ═╦════╦══
      ║    │
      ║    O
      ║   /│\
      ║
      ║
      ║
    ╔═║═╗
",
            @"
     ═╦════╦══
      ║    │
      ║    O
      ║   /│\
      ║   /
      ║
      ║
    ╔═║═╗
",
            @"
     ═╦════╦══
      ║    │
      ║    O
      ║   /│\
      ║   / \
      ║
      ║
    ╔═║═╗
            GAME OVER
"
        };
    }
}
