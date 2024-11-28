package domain

import "net"

type Player struct {
	ID   int
	Name string
	Conn net.Conn
}

func NewPlayer(conn net.Conn, id int, name string) *Player {
	return &Player{
		ID:   id,
		Name: name,
		Conn: conn,
	}
}
