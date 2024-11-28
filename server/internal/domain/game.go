package domain

import "time"

type Game struct {
	Word         string
	GuessedWord  []rune
	AttemptsLeft int
	StartTime    time.Time
}

func NewGame(word string, attempts int) *Game {
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
		if r == letter {
			g.GuessedWord[i] = letter
			found = true
		}
	}
	return found
}

// Проверка, угадано ли слово
func (g *Game) IsWordGuessed() bool {
	for _, r := range g.Word {
		if !contains(g.GuessedWord, r) {
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
