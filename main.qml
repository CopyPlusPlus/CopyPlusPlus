import QtQuick 2.12
import QtQuick.Window 2.12
import QtQuick.Controls 2.12

Window {
    id: window
    width: 400
    height: 300
    visible: true
    title: qsTr("Hello World")

    Switch {
        id: switch1
        x: 68
        y: 40
        text: qsTr("开关1")
        font.bold: true
    }

    Switch {
        id: switch2
        x: 68
        y: 85
        text: qsTr("开关2")
        font.bold: true
    }

    Switch {
        id: switch3
        x: 68
        y: 129
        text: qsTr("开关3")
        font.bold: true
    }
}
