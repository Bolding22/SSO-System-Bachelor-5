# IDENTITY PROVIDER

echo "*** IDENTITY PROVIDER ***"
openssl genrsa -out idp.key 2048
openssl req -new -key idp.key -out idp.csr -passout pass:1234

echo "*** Creating config ***"

# Create config file for certificate
# https://deliciousbrains.com/ssl-certificate-authority-for-local-https-development/#creating-ca-signed-certificates
cat << EOF > idp.ext
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
subjectAltName = @alt_names

[alt_names]
DNS.1 = idp
DNS.2 = localhost
EOF

echo "*** Creating new certificate for Identity Provider ***"

openssl x509 -req -in idp.csr -CA myCA.pem -CAkey myCA.key -CAcreateserial -out idp.crt -days 825 -sha256 -extfile idp.ext -passin pass:1234
openssl pkcs12 -inkey idp.key -in idp.crt -export -out idp.pfx

# SERVICE PROVIDER

echo "*** SERVICE PROVIDER ***"
openssl genrsa -out sp.key 2048
openssl req -new -key sp.key -out sp.csr -passout pass:1234

echo "*** Creating config ***"

# Create config file for certificate
# https://deliciousbrains.com/ssl-certificate-authority-for-local-https-development/#creating-ca-signed-certificates
cat << EOF > sp.ext
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
subjectAltName = @alt_names

[alt_names]
DNS.1 = sp
DNS.2 = localhost
DNS.3 = sub.localhost
EOF

echo "*** Creating new certificate for Service Provider ***"

openssl x509 -req -in sp.csr -CA myCA.pem -CAkey myCA.key -CAcreateserial -out sp.crt -days 825 -sha256 -extfile idp.ext -passin pass:1234
openssl pkcs12 -inkey sp.key -in sp.crt -export -out sp.pfx
