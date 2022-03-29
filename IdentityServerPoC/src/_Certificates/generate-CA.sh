echo "*** CERTIFICATE AUTHORITY ***"
openssl genrsa -des3 -passout pass:1234 -out myCA.key 2048
openssl req -x509 -new -nodes -key myCA.key -sha256 -days 1825 -out myCA.pem -passin pass:1234

# Add certificate to windows
# https://deliciousbrains.com/ssl-certificate-authority-for-local-https-development/#adding-root-cert-macos-keychain