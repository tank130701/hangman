package domain

type ClientKey struct {
	ConnAddr string
	Username string
	Password string
}

func NewClientKey(connAddr, username, password string) ClientKey {
	return ClientKey{
		ConnAddr: connAddr,
		Username: username,
		Password: password,
	}
}
