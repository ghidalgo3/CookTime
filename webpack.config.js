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
          ],
    },
    plugins: [
        // new CopyWebpackPlugin(
        //     {
        //         patterns: [
        //             { from: './node_modules/@fortawesome/fontawesome-free/css/all.min.css', to: '_fontawesome.min.css' },
        //             { from: './node_modules/@fortawesome/fontawesome-free/webfonts/*', to: '../webfonts/', flatten: true },
        //             { from: './node_modules/react/umd/react.production.min.js', to: '_react.production.min.js' },
        //             { from: './node_modules/react-dom/umd/react-dom.production.min.js', to: '_react-dom.production.min.js' },
        //             { from: './node_modules/bootstrap/dist/css/bootstrap.min.css', to: '_bootstrap.min.css' },
        //             { from: './node_modules/bootstrap/dist/js/bootstrap.min.js', to: '_bootstrap.min.js' },
        //             { from: './node_modules/jquery/dist/jquery.min.js', to: '_jquery.min.js' },
        //             { from: "./node_modules/popper.js/dist/umd/popper.min.js", to: "_popper.min.js" },
        //             { from: "./node_modules/jquery-validation/dist/jquery.validate.min.js", to: "_jquery.validate.min.js" },
        //             { from: "./node_modules/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.min.js", to: "_jquery.validate.unobtrusive.min.js" },
        //             { from: './node_modules/popper.js/dist/umd/popper.min.js.map' },
        //             { from: './node_modules/bootstrap/dist/css/bootstrap.min.css.map' },
        //             { from: './node_modules/bootstrap/dist/js/bootstrap.min.js.map' }
        //         ]
        //     }
        // )
    ]
};