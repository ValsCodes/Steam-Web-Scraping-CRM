# Stage 1: Build the Angular app
FROM node:alpine AS build

WORKDIR /usr/src/app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build --prod

# Stage 2: Serve the app with NGINX
FROM nginx:alpine

# Copy built Angular app to NGINX's web root
COPY --from=build /usr/src/app/dist/steam-app-angular-client/ /usr/share/nginx/html

# Expose default HTTP port
EXPOSE 4200

# Start NGINX server
CMD ["nginx", "-g", "daemon off;"]