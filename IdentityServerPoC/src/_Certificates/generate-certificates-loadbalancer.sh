# LOADBALANCER

echo "*** LOADBALANCER ***"
openssl genrsa -out loadbalancer.key 2048
openssl req -new -key loadbalancer.key -out loadbalancer.csr -passout pass:1234

echo "*** Creating config ***"

# Create config file for certificate
# https://deliciousbrains.com/ssl-certificate-authority-for-local-https-development/#creating-ca-signed-certificates
cat << EOF > loadbalancer.ext
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
subjectAltName = @alt_names

[alt_names]
DNS.1 = loadbalancer
DNS.2 = localhost
DNS.3 = *.localhost
EOF

echo "*** Creating new certificate for Loadbalancer ***"

openssl x509 -req -in loadbalancer.csr -CA myCA.pem -CAkey myCA.key -CAcreateserial -out loadbalancer.crt -days 825 -sha256 -extfile loadbalancer.ext -passin pass:1234
openssl pkcs12 -inkey loadbalancer.key -in loadbalancer.crt -export -out loadbalancer.pfx
