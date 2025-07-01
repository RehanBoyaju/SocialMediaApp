FROM nginx:alpine

COPY Published/wwwroot /usr/share/nginx/html

EXPOSE 80   
CMD ["nginx","-g","daemon off;"]


#docker run -d -p 5228:80 --name frontend blazorchat
