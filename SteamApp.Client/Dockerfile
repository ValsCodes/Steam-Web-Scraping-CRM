# Stage 1: Build the Angular app
FROM node:alpine AS build

WORKDIR /usr/src/app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build -- --configuration production

# Stage 2: Serve the app with NGINX
FROM nginx:alpine

# Copy built Angular app to NGINX's web root
COPY --from=build /usr/src/app/dist/steam-app-angular-client/ /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose default HTTP port
EXPOSE 80

# Start NGINX server
CMD ["nginx", "-g", "daemon off;"]
