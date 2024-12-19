package repository

import (
	"encoding/json"
	"errors"
	"math/rand"
	"os"
	"time"
)

// WordsRepository отвечает за хранение слов для игры.
type WordsRepository struct {
	categories   map[string][]string
	randomSource *rand.Rand // Генератор случайных чисел
}

// NewWordsRepository создает новый экземпляр WordsRepository и загружает слова из JSON-файла.
func NewWordsRepository(filePath string) (*WordsRepository, error) {
	file, err := os.Open(filePath)
	if err != nil {
		return nil, err
	}
	defer file.Close()

	var categories map[string][]string
	if err := json.NewDecoder(file).Decode(&categories); err != nil {
		return nil, err
	}

	if len(categories) == 0 {
		return nil, errors.New("no categories found in the file")
	}

	return &WordsRepository{
		categories:   categories,
		randomSource: rand.New(rand.NewSource(time.Now().UnixNano())),
	}, nil
}

// GetRandomWord возвращает случайное слово из указанной категории.
func (ws *WordsRepository) GetRandomWord(category string) (string, error) {
	words, err := ws.GetAllWords(category)
	if err != nil {
		return "", err
	}

	index := ws.randomSource.Intn(len(words))

	return words[index], nil
}

// GetAllWords возвращает все слова из указанной категории.
func (ws *WordsRepository) GetAllWords(category string) ([]string, error) {
	words, ok := ws.categories[category]
	if !ok {
		return nil, errors.New("category not found")
	}
	return words, nil
}

// GetCategories возвращает список всех доступных категорий.
func (ws *WordsRepository) GetCategories() []string {
	categories := make([]string, 0, len(ws.categories))
	for category := range ws.categories {
		categories = append(categories, category)
	}
	return categories
}

// GetAttempts рассчитывает количество попыток в зависимости от длины слова и уровня сложности.
func (ws *WordsRepository) GetAttempts(word string, difficulty string) int {
	length := len(word)
	baseAttempts := 0

	switch difficulty {
	case "easy":
		baseAttempts = 10
	case "medium":
		baseAttempts = 7
	case "hard":
		baseAttempts = 5
	default:
		baseAttempts = 7 // стандартный уровень сложности
	}

	// Дополнительные попытки для коротких слов
	if length <= 4 {
		baseAttempts += 2
	} else if length >= 10 {
		baseAttempts -= 1
	}

	if baseAttempts < 1 {
		baseAttempts = 1 // Минимум 1 попытка
	}

	return baseAttempts
}
