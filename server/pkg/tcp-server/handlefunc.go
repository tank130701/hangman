package tcp_server

import "context"

type HandleFunc func(ctx context.Context, message []byte) ([]byte, error)
