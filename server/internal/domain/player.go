package domain

import "net"

type Player struct {
	Username string
	Conn     net.Conn
}

func NewPlayer(conn net.Conn, id int, name string) *Player {
	return &Player{
		Username: name,
		Conn:     conn,
	}
}
