package tcp_server

import "net"

type HandleFunc func(conn net.Conn, message []byte) ([]byte, error)
