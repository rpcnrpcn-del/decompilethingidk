const EventEmitter = require('events')
const internalBus = new EventEmitter()
module.exports = internalBus