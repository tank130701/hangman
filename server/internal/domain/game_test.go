package domain

import (
	"testing"
)

func TestNewGame(t *testing.T) {
	word := "golang"
	attempts := 5
	game := NewGame(word, attempts)

	if game.Word != word {
		t.Errorf("Expected Word to be %s, got %s", word, game.Word)
	}
	if len(game.GuessedWord) != len(word) {
		t.Errorf("Expected GuessedWord length to be %d, got %d", len(word), len(game.GuessedWord))
	}
	if game.AttemptsLeft != attempts {
		t.Errorf("Expected AttemptsLeft to be %d, got %d", attempts, game.AttemptsLeft)
	}
}

func TestDisplayWord(t *testing.T) {
	game := NewGame("golang", 5)
	game.GuessedWord = []rune{'g', 'o', 0, 0, 0, 'g'}

	expected := "go___g"
	actual := game.DisplayWord()
	if actual != expected {
		t.Errorf("Expected DisplayWord to be %s, got %s", expected, actual)
	}
}

func TestUpdateGuessedWord(t *testing.T) {
	game := NewGame("golang", 5)
	found := game.UpdateGuessedWord('g')

	if !found {
		t.Errorf("Expected UpdateGuessedWord to return true for letter 'g'")
	}
	if game.GuessedWord[0] != 'g' || game.GuessedWord[5] != 'g' {
		t.Errorf("Expected 'g' to be updated in GuessedWord")
	}

	notFound := game.UpdateGuessedWord('z')
	if notFound {
		t.Errorf("Expected UpdateGuessedWord to return false for letter 'z'")
	}
}

func TestIsWordGuessed(t *testing.T) {
	game := NewGame("golang", 5)
	game.GuessedWord = []rune{'g', 'o', 'l', 'a', 'n', 'g'}

	if !game.IsWordGuessed() {
		t.Errorf("Expected IsWordGuessed to return true for complete word")
	}

	game.GuessedWord = []rune{'g', 'o', 'l', 'a', '_', '_'}
	if game.IsWordGuessed() {
		t.Errorf("Expected IsWordGuessed to return false for incomplete word")
	}
}

func TestContains(t *testing.T) {
	slice := []rune{'a', 'b', 'c'}
	if !contains(slice, 'a') {
		t.Errorf("Expected contains to return true for 'a'")
	}
	if contains(slice, 'z') {
		t.Errorf("Expected contains to return false for 'z'")
	}
}
