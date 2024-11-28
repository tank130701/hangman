package utils

import (
	"log"
	"os"
)

var Logger = log.New(os.Stdout, "", log.Ldate|log.Ltime|log.Lshortfile)
