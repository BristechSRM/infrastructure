case $1 in
  "comms") echo 9000 ;;
  "sessions") echo 9001 ;;
  "api-gateway") echo 9002 ;;
  "auth") echo 9003 ;;
	*) echo "INVALID_SERVICE_ID"
     exit 1;;
esac
