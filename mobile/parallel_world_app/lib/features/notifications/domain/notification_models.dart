class AppNotification {
  const AppNotification({
    required this.id,
    required this.category,
    required this.title,
    required this.resourceId,
    required this.isRead,
    required this.createdAtUtc,
  });

  final String id;
  final String category;
  final String title;
  final String resourceId;
  final bool isRead;
  final DateTime createdAtUtc;

  AppNotification copyWith({bool? isRead}) => AppNotification(
    id: id,
    category: category,
    title: title,
    resourceId: resourceId,
    isRead: isRead ?? this.isRead,
    createdAtUtc: createdAtUtc,
  );

  factory AppNotification.fromJson(Map<String, Object?> json) =>
      AppNotification(
        id: json['id']! as String,
        category: json['category']! as String,
        title: json['title']! as String,
        resourceId: json['resourceId']! as String,
        isRead: json['isRead']! as bool,
        createdAtUtc: DateTime.parse(json['createdAtUtc']! as String).toUtc(),
      );
}

class NotificationPage {
  const NotificationPage({
    required this.items,
    required this.nextCursor,
    required this.hasMore,
    this.isOffline = false,
  });

  final List<AppNotification> items;
  final String? nextCursor;
  final bool hasMore;
  final bool isOffline;
}
