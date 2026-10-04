enum MessageLocalState { synced, pending, failed }

class ConversationCharacter {
  const ConversationCharacter({
    required this.id,
    required this.actorId,
    required this.displayName,
    required this.handle,
  });
  final String id;
  final String actorId;
  final String displayName;
  final String handle;
  factory ConversationCharacter.fromJson(Map<String, dynamic> j) =>
      ConversationCharacter(
        id: j['id'] as String,
        actorId: j['actorId'] as String,
        displayName: j['displayName'] as String,
        handle: j['handle'] as String,
      );
}

class ConversationSummary {
  const ConversationSummary({
    required this.id,
    required this.character,
    required this.createdAtUtc,
    required this.lastMessageAtUtc,
    this.lastMessagePreview,
    required this.unreadCount,
    this.characterReplyStatus,
  });
  final String id;
  final ConversationCharacter character;
  final DateTime createdAtUtc;
  final DateTime lastMessageAtUtc;
  final String? lastMessagePreview;
  final int unreadCount;
  final String? characterReplyStatus;
  factory ConversationSummary.fromJson(Map<String, dynamic> j) =>
      ConversationSummary(
        id: j['id'] as String,
        character: ConversationCharacter.fromJson(
          j['character'] as Map<String, dynamic>,
        ),
        createdAtUtc: DateTime.parse(j['createdAtUtc'] as String).toUtc(),
        lastMessageAtUtc: DateTime.parse(j['lastMessageAtUtc'] as String)
            .toUtc(),
        lastMessagePreview: j['lastMessagePreview'] as String?,
        unreadCount: j['unreadCount'] as int? ?? 0,
        characterReplyStatus: j['characterReplyStatus'] as String?,
      );
}

class ConversationMessage {
  const ConversationMessage({
    required this.id,
    required this.conversationId,
    required this.senderActorId,
    required this.senderType,
    required this.body,
    required this.createdAtUtc,
    required this.deliveryStatus,
    this.clientMessageId,
    this.localState = MessageLocalState.synced,
    this.failureMessage,
  });
  final String id;
  final String conversationId;
  final String senderActorId;
  final String senderType;
  final String body;
  final DateTime createdAtUtc;
  final String deliveryStatus;
  final String? clientMessageId;
  final MessageLocalState localState;
  final String? failureMessage;
  factory ConversationMessage.fromJson(Map<String, dynamic> j) =>
      ConversationMessage(
        id: j['id'] as String,
        conversationId: j['conversationId'] as String,
        senderActorId: j['senderActorId'] as String,
        senderType: j['senderType'] as String,
        body: j['body'] as String,
        createdAtUtc: DateTime.parse(j['createdAtUtc'] as String).toUtc(),
        deliveryStatus: j['deliveryStatus'] as String,
        clientMessageId: j['clientMessageId'] as String?,
      );
  ConversationMessage copyWith({
    String? id,
    MessageLocalState? localState,
    String? failureMessage,
    bool clearFailure = false,
  }) => ConversationMessage(
    id: id ?? this.id,
    conversationId: conversationId,
    senderActorId: senderActorId,
    senderType: senderType,
    body: body,
    createdAtUtc: createdAtUtc,
    deliveryStatus: deliveryStatus,
    clientMessageId: clientMessageId,
    localState: localState ?? this.localState,
    failureMessage: clearFailure ? null : failureMessage ?? this.failureMessage,
  );
}

class ConversationPage {
  const ConversationPage(this.items, this.nextCursor, this.hasMore);
  final List<ConversationSummary> items;
  final String? nextCursor;
  final bool hasMore;
}

class MessagePage {
  const MessagePage(this.items, this.nextCursor, this.hasMore);
  final List<ConversationMessage> items;
  final String? nextCursor;
  final bool hasMore;
}

class SendMessageResponse {
  const SendMessageResponse(this.message, this.characterReplyStatus);
  final ConversationMessage message;
  final String characterReplyStatus;
}
