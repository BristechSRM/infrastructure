uuid=$(uuidgen)
# Set env variables for email, firstname & surname

aws dynamodb put-item --endpoint-url http://dynamodb:7000 \
  --table-name Auth.Users \
  --item \
      "{\"Id\":{\"S\":\"$uuid\"},\"Email\":{\"S\":\"$email\"},\"Firstname\":{\"S\":\"$firstname\"},\"Surname\":{\"S\":\"$surname\"}}"
