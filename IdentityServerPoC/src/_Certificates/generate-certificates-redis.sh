# REDIS

echo "*** REDIS ***"
openssl genrsa -out redis.key 2048
openssl req -new -key redis.key -out redis.csr -passout pass:1234

echo "*** Creating config ***"

# Create config file for certificate
# https://deliciousbrains.com/ssl-certificate-authority-for-local-https-development/#creating-ca-signed-certificates
cat << EOF > redis.ext
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
subjectAltName = @alt_names

[alt_names]
DNS.1 = cache
EOF

echo "*** Creating new certificate for redis ***"

openssl x509 -req -in redis.csr -CA myCA.pem -CAkey myCA.key -CAcreateserial -out redis.crt -days 825 -sha256 -extfile redis.ext -passin pass:1234
openssl pkcs12 -inkey redis.key -in redis.crt -export -out redis.pfx
