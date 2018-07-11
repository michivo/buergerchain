const Datastore = require('@google-cloud/datastore');

const datastore = Datastore();

var visitorCount = 0;

function sayHelloInGerman() {
    const visit = {
        timestamp: new Date(),
        count: visitorCount++
    }
    datastore.save({
        key: datastore.key('visitors'),
        data: visit
    });

    const query = datastore.createQuery('visitors');

    return datastore.runQuery(query)
        .then((results) => {
            const recent = results[0];
            return recent[recent.length - 1].count;
        })
}

module.exports = {
    sayHelloInGerman: sayHelloInGerman
}