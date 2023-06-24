(function () {
    if (!Array.prototype.move) {
        Array.prototype.move = function (old_index, new_index) {
            if (this.length === 0) {
                return this;
            }
            while (old_index < 0) {
                old_index += this.length;
            }
            while (new_index < 0) {
                new_index += this.length;
            }
            if (new_index >= this.length) {
                var k = new_index - this.length;
                while ((k--) + 1) {
                    this.push(undefined);
                }
            }
            this.splice(new_index, 0, this.splice(old_index, 1)[0]);
            return this; // for testing purposes
        };
    }

    if (!Date.prototype.removeTime) {
        Date.prototype.removeTime = function () {
            return new Date(
                this.getFullYear(),
                this.getMonth(),
                this.getDate()
            );
        };
    };
})(this);

var DatetimeExtension = {
    dateDiff: function (from, to, type) {
        let difference = from.getTime() - to.getTime();

        if (type === 'days') {
            return Math.ceil(difference / (1000 * 3600 * 24));
        }

        if (type === 'hours') {
            return Math.ceil(difference / (1000 * 3600 * 24) / 24);
        }

        if (type === 'minutes') {
            return Math.ceil(difference / (1000 * 3600 * 24) / 24 / 60);
        }

        return difference;
    }
};