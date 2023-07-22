import React, { Component } from 'react';
import './page.css';

export class Echo extends React.Component {
    static displayName = Echo.name;
    constructor(props) {
        super(props);
        this.state = { sender: "", message: "", chatMessages: [], nameSet: false }
        this.handleSubmit = this.handleSubmit.bind(this);
        this.setSenderOnChange = this.setSenderOnChange.bind(this);
        this.setMessageOnChange = this.setMessageOnChange.bind(this);
        this.connect = this.connect.bind(this);
        this.listRef = React.createRef();
    }

    handleSubmit(event) {
        event.preventDefault();
        if (this.connection.readyState !== 1) {
            this.connect();
        }

        if (this.state.sender) {
            this.setState({ nameSet: true });
        }

        if (this.state.sender && this.state.message) {
            this.connection.send(`{"Sender": "${this.state.sender}", "ChatMessage": "${this.state.message}"}`);
        }

       
    }

    setSenderOnChange(event) {
        this.setState({ sender: event.target.value });
    }

    setMessageOnChange(event) {
        this.setState({ message: event.target.value });
    }

    connect() {
        this.connection = new WebSocket('wss://websocketchatting.azurewebsites.net/websocket/websocketget');
        this.connection.onmessage = evt => {
            const chatMessages = JSON.parse(evt.data);
            this.setState({
                chatMessages: chatMessages
            });
        };
      
        this.connection.onclose = function(e) {
          console.log('Socket is closed. Reconnect will be attempted in 1 second.', e.reason);
          setTimeout(function() {
            this.connect();
          }, 1000);
        };
      
        this.connection.onerror = function(err) {
          console.error('Socket encountered error: ', err.message, 'Closing socket');
          this.connection.close();
        };
    }
      

    componentDidMount() {
        this.connect();
    }

    componentDidUpdate() {
        const listContainer = this.listRef.current;
        if(listContainer) {
            listContainer.scrollTop = listContainer.scrollHeight - listContainer.clientHeight;
        }
    }

    render() {
        // slice(-5) gives us the five most recent messages
        if (this.state.nameSet) {

            return <div class="page-outer">
                <h1 class="blue-text-heading">Group Chat</h1>
                <div class="page"  ref={this.listRef}>
                {this.state.chatMessages.slice(0, 25).reverse().map((msg, idx) =>
                    <div key={idx} class="item">
                        <span class="blue-text">{msg.Sender}</span>:
                        <span class="red-text"> {msg.ChatMessage} </span>
                    </div>
                )}
                <br />
                <form class="form-container" onSubmit={this.handleSubmit}>
                        <label>
                        <span class="blue-text">{this.state.sender}</span>
                        <input
                            type="text"
                            value={this.state.message}
                            onChange={this.setMessageOnChange}
                            required
                            class="form-input-message"
                        />
                    </label>
                    <br />
                    <button type="submit" class="submit-button">Send</button>
                </form>
                </div>
            </div>
        }  else {
            return <div class="page-outer">
                <h1 class="blue-text-heading">Group Chat</h1>
                
                <form class="form-container" onSubmit={this.handleSubmit}>
                    <label>
                        <span class="blue-text">Name</span>
                        <input
                            type="text"
                            value={this.state.sender}
                            onChange={this.setSenderOnChange}
                            required
                            class="form-input-name"
                        />
                    </label>
                    <br />
                    <button type="submit" class="submit-button">Send</button>
                </form>
            </div>
        }
    }
}

