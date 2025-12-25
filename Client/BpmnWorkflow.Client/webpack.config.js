const path = require('path');

module.exports = {
    mode: 'development',
    entry: './src/camunda-modeler-entry.js',
    output: {
        path: path.resolve(__dirname, 'wwwroot/js'),
        filename: 'camunda-modeler-bundle.js',
        library: {
            name: 'CamundaModeler',
            type: 'window',
        },
    },
    module: {
        rules: [
            {
                test: /\.css$/i,
                use: ['style-loader', 'css-loader'],
            },
            {
                test: /\.(png|jpe?g|gif|svg|eot|ttf|woff|woff2)$/i,
                type: 'asset/resource',
            },
            {
                test: /\.bpmn$/,
                type: 'asset/source',
            },
        ],
    },
    devtool: 'source-map'
};
