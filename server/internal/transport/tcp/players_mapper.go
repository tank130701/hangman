package tcp

import (
	"hangman/internal/domain"
	"net"
)

func ConvertPlayersToSlice(players map[string]*domain.Player) []PlayerDTO {
	playerSlice := make([]PlayerDTO, 0, len(players))
	for username, player := range players {
		playerSlice = append(playerSlice, PlayerDTO{
			Username:    username,
			Score:       player.Score,
			IsConnected: player.IsConnected,
		})
	}
	return playerSlice
}

func GetPlayerIp(connAddr string) (string, error) {
	connIp, _, err := net.SplitHostPort(connAddr)
	if err != nil {
		return "", err
	}
	return connIp, nil
}
