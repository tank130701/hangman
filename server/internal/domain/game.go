package domain

type Game struct {
	Word         string
	GuessedWord  []rune
	AttemptsLeft int
	Score        int
}

func NewGame(word string, attempts int) *Game {
	guessedWord := make([]rune, len(word))
	for i, r := range word {
		if r == '-' {
			guessedWord[i] = '-' // Дефисы сразу считаются угаданными
		}
	}
	return &Game{
		Word:         word,
		GuessedWord:  make([]rune, len(word)),
		AttemptsLeft: attempts,
	}
}

// Проверка текущего состояния слова
func (g *Game) DisplayWord() string {
	displayed := make([]rune, len(g.Word))
	for i, r := range g.Word {
		if contains(g.GuessedWord, r) {
			displayed[i] = r
		} else {
			displayed[i] = '_'
		}
	}
	return string(displayed)
}

// Обновление угаданного слова
func (g *Game) UpdateGuessedWord(letter rune) bool {
	found := false
	for i, r := range g.Word {
		if r == letter && g.GuessedWord[i] != letter {
			g.GuessedWord[i] = letter
			found = true
		}
	}
	return found
}

// Проверка, угадано ли слово
func (g *Game) IsWordGuessed() bool {
	for i, r := range g.Word {
		if r != '-' && g.GuessedWord[i] != r {
			return false
		}
	}
	return true
}

// Проверка наличия символа в срезе
func contains(slice []rune, r rune) bool {
	for _, v := range slice {
		if v == r {
			return true
		}
	}
	return false
}
