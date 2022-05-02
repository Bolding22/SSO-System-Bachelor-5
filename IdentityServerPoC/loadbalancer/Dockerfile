FROM nginx
RUN rm /etc/nginx/conf.d/default.conf
RUN mkdir /etc/nginx/ssl
COPY loadbalancer/certificates/loadbalancer.crt /etc/nginx/ssl
COPY loadbalancer/certificates/loadbalancer.key /etc/nginx/ssl
COPY loadbalancer/nginx.conf /etc/nginx/conf.d/default.conf

RUN mkdir /usr/local/share/ca-certificates/extra
ADD src/_Certificates/myCA.pem /usr/local/share/ca-certificates/extra/myCA.crt
RUN update-ca-certificates