const path = require('path');

module.exports = {
    devtool: 'inline-source-map',
    devServer: {
        static: './wwwroot/js',
        hot: true,
    },
    entry: {
        recipeEdit: './Scripts/recipeEdit.tsx',
        cart: './Scripts/Cart.tsx',
        ingredients: './Scripts/Ingredients.tsx',
        registration: './Scripts/Registration.ts',
        css: './wwwroot/css/site.css',
    },
    output: {
        publicPath: '/',
        path: path.resolve(__dirname, 'wwwroot/js'),
        filename: '[name].js'
    },
    resolve: {
        extensions: [".js", ".ts", ".tsx"]
    },
    module: {
        rules: [
            {
              test: /\.tsx?$/,
              use: 'ts-loader',
              exclude: /node_modules/,
            },
            {
                test: /\.css$/i,
                use: ["style-loader", "css-loader"],
            },
          ],
    },
    plugins: [
    ]
};