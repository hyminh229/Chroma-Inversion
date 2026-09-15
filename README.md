# HTSpaceShooter
Học phần: Đồ án thực tế công nghệ phần mềm
CHƯƠNG 1. TỔNG QUAN VỀ ĐỀ TÀI
1.1. GIỚI THIỆU ĐỀ TÀI
Trong bối cảnh ngành công nghiệp game 2D indie đang phát triển mạnh mẽ, thể loại bắn súng không gian (Space Shooter) luôn giữ được sức hút nhờ lối chơi nhịp độ cao, phản xạ nhanh và tính giải trí trực quan. Đề tài "Space Shooter – Chroma Inversion" là một sản phẩm trò chơi điện tử 2D thuộc thể loại arcade không gian, được phát triển trên nền tảng Unity.  
Điểm nhấn cốt lõi của đề tài là cơ chế đảo chiều sắc thái màu sắc (Chroma Inversion – chuyển đổi qua lại giữa hai hệ màu Xanh và Đỏ). Người chơi không chỉ phải né tránh đạn và tiêu diệt kẻ địch mà còn phải chủ động thay đổi trạng thái màu sắc của tàu chiến để hấp thụ năng lượng, kích hoạt các kỹ năng tối thượng (Mega Beam) hoặc tương tác với môi trường màn chơi. Trò chơi kết hợp phong cách đồ họa hoài cổ (Pixel Art) với các kỹ thuật lập trình hiện đại, mang lại trải nghiệm hoài niệm nhưng đầy tính chiến thuật. 
1.2. LÝ DO CHỌN ĐỀ TÀI
Lý do nhóm quyết định lựa chọn đề tài này xuất phát từ các yếu tố thực tiễn và học thuật sau:
Khám phá và làm chủ công nghệ Unity 2D: Unity là một trong những engine game phổ biến và mạnh mẽ nhất hiện nay. Đề tài là cơ hội để nhóm áp dụng các kiến thức chuyên ngành về xử lý đồ họa sprite, hệ thống quản lý animation, quản lý va chạm (Physics 2D) và tối ưu hóa hiệu suất thời gian thực.
Thử thách với cơ chế gameplay sáng tạo: Thay vì lối chơi bắn súng truyền thống (chỉ cần né và bắn), cơ chế đổi màu (Chroma Inversion) đòi hỏi tư duy logic trong việc thiết kế hệ thống trạng thái (State Machine), đồng bộ hóa chặt chẽ giữa logic lập trình và tài nguyên đồ họa.
Phát triển toàn diện kỹ năng làm game độc lập (Indie Game Development): Đề tài đòi hỏi sự phối hợp nhịp nhàng giữa hai thành viên trong việc phân chia rõ ràng các giai đoạn: từ nghiên cứu kiến trúc, thiết kế toàn bộ tài nguyên đồ họa dạng Pixel Art, quản lý mã nguồn qua Git/GitHub, cho đến việc xây dựng hoàn chỉnh một sản phẩm có thể đóng gói và vận hành mượt mà (Build APK).

1.3. MỤC TIÊU CỦA ĐỀ TÀI
Đề tài hướng tới việc hoàn thành các mục tiêu cụ thể sau:
Về mặt sản phẩm (Game hoàn thiện):
Xây dựng thành công một tựa game Space Shooter 2D hoàn chỉnh với hệ thống màn chơi, các chủng loại kẻ địch đa dạng (quái hình khối, quái bắn đạn, thiên thạch) và hệ thống boss.  
Hiện thực hóa mượt mà cơ chế đổi màu (Chroma Inversion), hệ thống thanh năng lượng, cơ chế thu thập vật phẩm (Power-up) và kỹ năng đặc biệt (Mega Beam). 
Về mặt kỹ thuật và đồ họa:
Thiết kế trọn bộ tài nguyên đồ họa theo phong cách Pixel Art chuẩn hóa (từ tàu chiến, hiệu ứng đạn, tia laser đến giao diện HUD).  
Ứng dụng các mẫu thiết kế phần mềm (Design Patterns) và kiến trúc lập trình tối ưu trong Unity để đảm bảo code dễ mở rộng và bảo trì.
Về mặt học thuật:
Hoàn thiện đầy đủ hồ sơ báo cáo đồ án, tài liệu thiết kế hệ thống, quy tắc quản lý asset, đáp ứng nghiêm ngặt các yêu cầu báo cáo học thuật được đề ra.
1.4. PHẠM VI VÀ GIỚI HẠN CỦA ĐỀ TÀI
Phạm vi nghiên cứu và phát triển:
Tập trung xây dựng hệ thống gameplay cốt lõi của một tựa game bắn súng không gian 2D (Space Shooter) chạy trên nền tảng PC/Desktop hoặc Android (thông qua file cài đặt .apk).
Phát triển trọn vẹn các cơ chế chính: hệ thống di chuyển, cơ chế đổi màu (Chroma Inversion), hệ thống tính điểm, vật phẩm hỗ trợ (Power-up), các đợt tấn công của kẻ địch (Wave 1 đến Wave 5) và đấu trùm (Boss).
Giới hạn đề tài:
Do thời gian thực hiện có hạn (20 ngày) và nguồn lực nhóm nhỏ, game tập trung vào chiều sâu trải nghiệm cơ chế gameplay và phong cách đồ họa Pixel Art thay vì mở rộng quá nhiều màn chơi lớn hay tích hợp hệ thống lưu trữ trực tuyến (Cloud Save/Multiplayer mạng trực tuyến).
1.5. Ý NGHĨA KHOA HỌC VÀ THỰC TIỄN CỦA ĐỀ TÀI
Ý nghĩa khoa học: Củng cố và minh chứng khả năng vận dụng các kiến thức về lập trình hướng đối tượng, kiến trúc mã nguồn trong game engine, quản lý vòng đời đối tượng (Object Pooling), cùng sự đồng bộ hóa giữa tư duy thiết kế đồ họa (Pixel Art) và logic lập trình thời gian thực.
Ý nghĩa thực tiễn: Tạo ra một sản phẩm game giải trí hoàn chỉnh có chất lượng thẩm mỹ cao, có thể đưa vào sử dụng thực tế, đồng thời làm tài liệu tham khảo cho các nhóm sinh viên khác có định hướng nghiên cứu và phát triển game độc lập (Indie Game) trên nền tảng Unity.

