FROM node:24-alpine 
WORKDIR /app

# Copy package files
COPY package*.json ./
RUN npm ci

# Copy source code
COPY . .

# Build the React app
RUN npm run build
