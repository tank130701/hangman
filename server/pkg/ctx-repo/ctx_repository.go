package ctx_repo

import (
	"context"
	"sync"
)

type ContextWithCancel struct {
	ctx    context.Context
	cancel context.CancelFunc
}

type CtxRepository struct {
	mu       sync.Mutex
	contexts map[string]ContextWithCancel
}

// NewCtxRepository создает новый экземпляр CtxRepository.
func NewCtxRepository() *CtxRepository {
	return &CtxRepository{
		contexts: make(map[string]ContextWithCancel),
	}
}

// UpdateOrInsertCtx создает новый контекст с отменой для указанного идентификатора.
func (repo *CtxRepository) UpdateOrInsertCtx(id string, ctx context.Context) {
	repo.mu.Lock()
	defer repo.mu.Unlock()

	// Если контекст уже существует, обновляем его
	if ctxWithCancel, exists := repo.contexts[id]; exists {
		ctxToUpdate, cancel := context.WithCancel(ctxWithCancel.ctx)
		repo.contexts[id] = ContextWithCancel{ctx: ctxToUpdate, cancel: cancel}
	}

	// Создаем новый контекст с отменой
	ctx, cancel := context.WithCancel(ctx)
	repo.contexts[id] = ContextWithCancel{ctx: ctx, cancel: cancel}
}

// GetContext возвращает контекст для указанного идентификатора.
func (repo *CtxRepository) GetContext(id string) (context.Context, bool) {
	repo.mu.Lock()
	defer repo.mu.Unlock()

	ctxWithCancel, exists := repo.contexts[id]
	if !exists {
		return nil, false
	}
	return ctxWithCancel.ctx, true
}

// CancelContext отменяет контекст для указанного идентификатора.
func (repo *CtxRepository) CancelContext(id string) {
	repo.mu.Lock()
	defer repo.mu.Unlock()

	if ctxWithCancel, exists := repo.contexts[id]; exists {
		ctxWithCancel.cancel()
		delete(repo.contexts, id) // Удаляем контекст из репозитория
	}
}
